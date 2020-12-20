using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Collections;

namespace SearchSameFiles                                                   {
    class                               Program                             {

        //TODO: by using our own tree instead , we can further optimizate it by avoid unecessary read on files by sparse reading 
        //      we may also implement equality by similar content instead of just identical
        SortedSet<FileDesc>             sortedFiles                         = new SortedSet<FileDesc>(new FileComparer(new SameLengthFileList()));


        private class                   ChunkDigest                         {
            static SHA1Managed          algo = new SHA1Managed();
            byte[]                      digest;

            internal void update(byte[] buff)   {
                digest=algo.ComputeHash(buff);
            }

            internal bool compare(ChunkDigest chunkDigest)  {
                return StructuralComparisons.StructuralEqualityComparer.Equals(digest,chunkDigest.digest);
            }

            //TODO:    complete this

        }

        private class                   SameLengthFileList                  {
            List<FileDesc>              sameLengthFiles = new List<FileDesc>();
            internal int                add                                 (FileDesc a,FileDesc b)                                         {
                //Log.dbg("add: {0} - {1}", a, b);
                insert(a);
                insert(b);
                return 0;// anyway 
            }

            private void insert(FileDesc x) {
               // sameLengthFiles.Add(x);
            }
        }

        private class                   FileDesc                            {
            FileInfo                    fileInfo;
            long?                       checkPos;       // we will use this as index of where comparison failed , (not necesarely the position from the begining (sparse search))
            byte?                       lastCheck;      // this is the first char wich differs we got .. (so .. checkPos+lastCheck are then use for sort files )
            ChunkDigest[]               digestMap;      // we'll optimize this later .. an array it's ok to begin with , will be also faster in case of sparse chunks check
            ISparseSeekHandler          seekHandler;   // drives sparse lookup ..

            public                      FileDesc                            (FileInfo fileInfo, ISparseSeekHandler seekHandler)             {
                this.fileInfo       =   fileInfo;
                this.seekHandler    =   seekHandler;
                
            }
            public override string      ToString                            ()                                                              {
                return this.GetType().Name+": " + fileInfo.Name;
            }

        

            internal long               len                                 ()                                                              {
                return fileInfo.Length;
            }

            internal int                compare                             (FileDesc b)                                                    {
                //Console.WriteLine("compare: [{0} - {1}]", fileInfo.Name, b.fileInfo.Name);
                long fLen = len();
                long lenDiff = len() - b.len();
                if (lenDiff < 0) return -1;
                else if (lenDiff > 0) return 1;
                else  {
                    Log.dbg("compare same length: [{0} - {1}]", fileInfo.Name, b.fileInfo.Name);

                    int chunkSzMax = seekHandler.getChunkSize();            //TODO: chose most efficient disk read size here !!

                    //TODO: use here previously stored compare states

                    long pos = 0; 

                    FileStream fsA  = fileInfo.OpenRead();
                    FileStream fsB  = b.fileInfo.OpenRead();
                    byte[] buffA    = new byte[chunkSzMax];
                    byte[] buffB    = new byte[chunkSzMax];
                    int res         = 0; // assume identical files
                    int i           = 0;
                    int chunkIndx   = 0;
                    int numChunks   = seekHandler.getNumChunks();
                    if (digestMap==null)        digestMap   = new ChunkDigest[numChunks];
                    if (b.digestMap == null)    b.digestMap = new ChunkDigest[numChunks];
                    while (pos < fLen) {

                        long remain = fLen - pos;
                        int chunkSz = remain > chunkSzMax ? chunkSzMax : (int)remain;

                        if (haveDifferentDigest(chunkIndx,b) ) {
                            // having a different digest means they are different for sure so we don't have to bother going on
                            // however we have to store correctly the state for the state here
                            Log.dbg("different digest: {0} <> {1} chunk: ", fileInfo.Name, b.fileInfo.Name,chunkIndx);
                            return -666;
                        }

                        int doneA = fsA.Read(buffA, 0, chunkSz);
                        int doneB = fsB.Read(buffB, 0, chunkSz);
                        //TODO: make sure read completed full chunk
                        //****    
                        setChunkDigest(chunkIndx, buffA);
                        b.setChunkDigest(chunkIndx, buffB);
                        for (i = 0; i < chunkSz; i++) {
                            int diff = buffA[i] - buffB[i];
                            if (diff < 0) {
                                res= -1;
                                goto endScan;
                            }
                            else if (diff > 0) {
                                res= 1;
                                goto endScan;
                            }
                        }
                        pos += chunkSz;
                        chunkIndx++;
                    }
                    endScan: // they are different
                    fsA.Close();
                    fsB.Close();

                    storeState(b, pos + i, buffA[i], buffB[i]);
                    if (res==0) Log.dbg("identical: {0} = {1}", fileInfo.Name, b.fileInfo.Name);
                    else        Log.dbg("different: at {0}  {1} != {2}",pos+i , buffA[i],buffB[i]);
                    return res;
                }
            }

            private bool                haveDifferentDigest                 (int idx,FileDesc b)                                            { // different or uncomputed
                return (digestMap[idx]!=null && b.digestMap[idx]!=null && !digestMap[idx].compare(b.digestMap[idx]));
            }

            private void                setChunkDigest                      (int chunkIndx, byte[] buff)                                    {
                if (digestMap[chunkIndx] == null) {
                    //Log.dbg("setChunkDigest::{0} : {1}", this, chunkIndx);
                    ChunkDigest digest = new ChunkDigest();
                    digest.update(buff);
                    digestMap[chunkIndx] = digest;
                }
            }

            private void                storeState                          (FileDesc b, long pos, byte v1, byte v2)                        {
                checkPos    =
                b.checkPos  = pos;
                lastCheck   = v1;
                b.lastCheck = v2;

            }

        }

        private class                   FileComparer 
            :                           IComparer<FileDesc>                                                                                 {
            private SameLengthFileList sameLengthFileList;

            public FileComparer(SameLengthFileList sorter)     {
                sameLengthFileList = sorter;
            }

            public int Compare([AllowNull] FileDesc a, [AllowNull] FileDesc b)  {
                int map  = a == null? 1: 0;
                    map += b == null? 2: 0;

                switch (map) {
                    case 1:     return -1;                                  
                    case 2:     return +1;                                  
                    case 3:     return  0;              // same NULL
                    default:
                                int res=a.compare(b);
                                // we tell they are equal and sort them inside , we don't really care about overall different size order
                                if (res==0) sameLengthFileList.add(a,b);
                                return res;
                                
                }
            }
        }

        private void                    treeAdd                             (string fName, ISparseSeekProvider seekProvider)                {
            FileInfo fi = new FileInfo(fName);
            sortedFiles.Add(new FileDesc(fi,seekProvider.getSeekHandler(fi)));
        }

        private List<List<FileDesc>>    collectSame                         (SortedSet<FileDesc> sortedFiles)                               {
            Log.err("TODO ");
            return null;
        }

        private List<List<FileDesc>>    findDuplicates                      (string[] files, ISparseSeekProvider seekProvider)              {
            //Log.dbg("found {0} files ..", files.Length);
            foreach(string fName in files) {
                treeAdd(fName, seekProvider);
            }
            return collectSame(sortedFiles);
        }


        private void                    run                                 (string path,string filter, ISparseSeekProvider seekProvider)   {
            try {
                Log.dbg("running: {0} on {1} filter: {2}", this,path,filter);
                string[] files = Directory.GetFiles(path);
                findDuplicates(files, seekProvider);
            }
            catch (Exception e)  {
                Log.err("Exception: {0}", e.Message);

            }
        }

        static void                     Main                                (string[] args)                                                 {
            int     min = 1
                ,   max = 666
                ;
            string  sampleFolderName    = "/___NODE/niMan";
            string  sampleFileFilter    = "";
            string  sampleFileName      = sampleFolderName+"/demo.gif";


            TestSeekProvider.test(typeof(BrokenSeekHandler)     , sampleFileName, min, max,TestForFailure.NO_SEEK);
            TestSeekProvider.test(typeof(LinearSeekProvider)    , sampleFileName, min, max);
            TestSeekProvider.test(typeof(ZigZagSeekProvider)    , sampleFileName, min, max);
            TestSeekProvider.test(typeof(HopperSeekProvider)   , sampleFileName, min, max);

            new Program().run(sampleFolderName, sampleFileFilter, new HopperSeekProvider(4096));
        }


    }



}

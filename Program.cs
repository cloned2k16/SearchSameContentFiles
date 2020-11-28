using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SearchSameFiles                                                   {
    class                               Program                             {

        //TODO: by using our own tree instead , we can further optimizate it by avoid unecessary read on files by sparse reading
        //      we may also implement equality by similar content instead of just identical
        SortedSet<FileDesc>             sortedFiles                         = new SortedSet<FileDesc>(new FileComparer());

        private class                   FileDesc                            {
            FileInfo    fileInfo;
            long?       sortChunk;      // for further optimization reading pseudo random chuncks (sparse)
            long?       checkPos;       // if they are same size we use this as
            byte?       lastCheck;


            public                      FileDesc                            (string file)                                                   {
                fileInfo = new FileInfo(file);

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

                    int chunkSzMax = 4096; //TODO: chose most efficient disk read size here !!

                    //TODO: use here previously stored compare states

                    long pos = 0;

                    FileStream fsA  = fileInfo.OpenRead();
                    FileStream fsB  = b.fileInfo.OpenRead();
                    byte[] buffA    = new byte[chunkSzMax];
                    byte[] buffB    = new byte[chunkSzMax];
                    int res         = 0; // assume identical files
                    int i           = 0;
                    while (pos < fLen) {

                        long remain = fLen - pos;
                        int chunkSz = remain > chunkSzMax ? chunkSzMax : (int)remain;
                        int doneA = fsA.Read(buffA, 0, chunkSz);
                        int doneB = fsB.Read(buffB, 0, chunkSz);
                        //TODO: make sure read completed full chunk
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
                    }
                    endScan: // they are different

                    storeState(b, pos + i, buffA[i], buffB[i]);
                    if (res==0) Log.dbg("identical: {0} = {1}", fileInfo.Name, b.fileInfo.Name);
                    else        Log.dbg("different: at {0}  {1} != {2}",pos , buffA[i],buffB[i]);
                    return res;
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
            public int Compare([AllowNull] FileDesc a, [AllowNull] FileDesc b)  {
                int map  = a == null? 1: 0;
                    map += b == null? 2: 0;

                switch (map) {
                    case 1:     return -1;
                    case 2:     return +1;
                    case 3:     return  0;              // same NULL
                    default:    return a.compare(b);
                }
            }
        }

        private void                    treeAdd                             (string fName)                                                  {
            Log.dbg("adding file: {0}", fName);
            sortedFiles.Add(new FileDesc(fName));
        }

        private List<List<FileDesc>>    collectSame                         (SortedSet<FileDesc> sortedFiles)                               {
            Log.err("TODO ");
            return null;
        }

        private List<List<FileDesc>>    findDuplicates                      (string[] files)                                                {
            Log.dbg("found {0} files ..", files.Length);
            foreach(string fName in files) {
                treeAdd(fName);
            }
            return collectSame(sortedFiles);
        }

        class                           Log                                 {
            internal static void dbg(String fmt, params object[] args)  {
                Console.WriteLine(fmt, args);
            }
            internal static void err(String fmt, params object[] args)  {
                Console.WriteLine("ERROR: " + fmt, args);
            }
        }

        private void                    run                                 (string path,string filter)                                     {
            try {
                Log.dbg("running: {0} on {1} filter: {2}", this,path,filter);
                string[] files = Directory.GetFiles(path);
                findDuplicates(files);
            }
            catch (Exception e)  {
                Log.err("Exception: {0}", e.Message);

            }
        }

        static void                     Main                                (string[] args)                                                 {
                new Program().run("/___NODE/niMan", "");
        }
    }
}

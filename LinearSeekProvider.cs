using System;
using System.IO;

namespace SearchSameFiles {
    public class                        LinearSeekProvider
                :                       ISparseSeekProvider                 {
        protected readonly int chunkSize;

        public                          LinearSeekProvider                  (int chunkSize)                                                 {
            this.chunkSize = chunkSize;
        }

        virtual
        public ISparseSeekHandler       getSeekHandler                      (FileInfo fileInfo) {
            return new LinearSeekHandler(fileInfo, chunkSize);
        }
    }

    public class                        LinearSeekHandler 
                :                       ISparseSeekHandler {
        protected bool                  done;
        protected readonly long         length;
        protected readonly int          chunkSz;
        protected readonly int          remain;                             // last chunk size  
        protected readonly int          maxIndex;
        protected int                   currentIndex;

        public                          LinearSeekHandler                   (FileInfo fileInfo, int chunkSize)                              {
            length                  =   fileInfo.Length;
            if ( length <= 0 )
                done                =   true;
            else {
                done                =   false;
                if ( chunkSize <= 0 ) throw new Exception("invalid chunk size: " + chunkSize);
                if ( chunkSz > length ) chunkSz = ( int )   length;
                else                    chunkSz =           chunkSize;
                currentIndex        =   0;
                long checkMax = length / chunkSz;
                if ( checkMax > int.MaxValue ) new Exception("much too small chunk size: " + chunkSz);
                maxIndex            =   ( int )checkMax-1;
                remain              =   ( int )( length % chunkSz );
                if ( 0 == remain ) remain = chunkSz;
                else maxIndex++;
            }
        }

        public int                      getChunkSize                        ()                                                              {
            return chunkSz;
        }

        public int                      getNumChunks                        ()                                                              {
            return maxIndex+1;
        }

        public bool                     hasNext                             ()                                                              {
            return !done;
        }

        public ChunkDesc                next                                ()                                                              {
            if ( done ) return null;
            else {
                ChunkDesc nxt = new ChunkDesc(currentIndex * chunkSz, currentIndex == maxIndex ? remain : chunkSz);
                stepToNext();
                return nxt;
            }
        }
        virtual
        protected void                  stepToNext                          ()                                                              {
            if ( currentIndex != maxIndex ) currentIndex++;
            else done = true;
        }
    }
}

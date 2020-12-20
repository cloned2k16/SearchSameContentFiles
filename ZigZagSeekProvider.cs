using System;
using System.IO;

namespace SearchSameFiles {             // a very simple sparse mapping POC

    public class                        ZigZagSeekProvider
                :                       LinearSeekProvider                 {

        public                          ZigZagSeekProvider                  (int chunkSize) : base(chunkSize)                               {}

        override
        public ISparseSeekHandler       getSeekHandler                      (FileInfo fileInfo)                                             {
            return new ZigZagSeekHandler (fileInfo, chunkSize);
        }

    }

    public class                        ZigZagSeekHandler
                :                       LinearSeekHandler                   {

        private          int            gap;

        public                          ZigZagSeekHandler                   (FileInfo fileInfo, int chunkSize) 
                                        :                                   base     (fileInfo,     chunkSize)                              {
            currentIndex        =   0;
            gap                 =   maxIndex;
        }

        protected override void         stepToNext                          ()                                                              {
                if ( gap!=0 ) {
                    currentIndex += gap;
                    gap = gap<0? -(++gap) : -(--gap);
                }
                else done = true;
        }
    }

}
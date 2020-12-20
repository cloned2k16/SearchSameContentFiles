using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SearchSameFiles {
    public class                        HopperSeekProvider
                :                       LinearSeekProvider {
        public                          HopperSeekProvider                  (int chunkSize) : base(chunkSize)                               {}

        override 
        public ISparseSeekHandler      getSeekHandler                       (FileInfo fileInfo)                                             {
            return new HopperSeekHandler(fileInfo, chunkSize);
        }

    }

    public class                        HopperSeekHandler 
                :                       LinearSeekHandler                   {
        
        private int                     gap;

        public                          HopperSeekHandler                   (FileInfo fileInfo, int chunkSize) 
                :                                                           base    ( fileInfo,     chunkSize)                              {
            currentIndex        =   0;
            gap                 =   2; //easy! ..  with some more effort we can jump over an abitrary gap
        }

        override
        protected  void                 stepToNext                          ()                                                              {
            if (maxIndex==0 || currentIndex==1) done = true; 
            else {
                currentIndex+=gap;
                if ( currentIndex > maxIndex ) {
                     currentIndex-= (maxIndex&1)==0?3:1;
                     gap=-gap; 
                }
            }
        }
    }
}

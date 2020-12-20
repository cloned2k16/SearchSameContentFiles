using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

// the  purpose of this is to verify correctnes of Tests
// we'll actually need to make one for each possible broken case

namespace SearchSameFiles {


    public class                        BrokenSeekProvider
                :                       LinearSeekProvider {

        public                          BrokenSeekProvider                  (int chunkSize) : base(chunkSize)                               { }

        override
        public ISparseSeekHandler       getSeekHandler                      (FileInfo fileInfo)                                             {
            return new BrokenSeekHandler(fileInfo, chunkSize);
        }

    }

    public class                        BrokenSeekHandler 
                :                       LinearSeekHandler                   {

        public                          BrokenSeekHandler           (FileInfo fileInfo, int chunkSize)
                                        :                           base (    fileInfo,     chunkSize)                                      {
        }

        override
        protected void                  stepToNext()   {
        }
    }
}

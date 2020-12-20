using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SearchSameFiles {

    enum                                TestForFailure                      { NONE, NULL_INSTANCE, NO_SEEK, MISSING_HIT, LOOP, /* etc ... */ }

    internal class                      TestSeekProvider                    
                :                       ITestFrameWork                     {

        private static TestForFailure   testForFailure;

        internal static void            test                                (Type classType, string testFileName, int minSplits, int maxSplits, TestForFailure expectFailure = TestForFailure.NONE )  {
            testForFailure = expectFailure;
            bool isFailureTest = classType.Name.StartsWith("Broken");
            try  {
                FileInfo fileInf = new FileInfo(testFileName);
                long fullSz = fileInf.Length;
                for ( int n = minSplits; n < maxSplits; n++ )  {
                    int chunkSz = ( int )( fullSz/ n );
                    ISparseSeekProvider provider = ( ISparseSeekProvider )Activator.CreateInstance(classType, chunkSz);
                    ISparseSeekHandler zzHandler = provider.getSeekHandler(fileInf);
                    FakeFile ff = new FakeFile(fileInf, chunkSz);
                    int stp = 1;
                    while ( zzHandler.hasNext() )  {
                        ChunkDesc zzDesc = zzHandler.next();
                        ff.hit(zzDesc);
                    }
                    ff.assertAllProcessed();
                }

                if (isFailureTest)      Log.err(classType.Name+" FAILED: completed without any exception !");
                else                    Log.dbg(classType.Name+" PASSED");
            }
            catch ( Exception e )  {
                if ( isFailureTest )    Log.dbg(classType.Name+" PASSED");
                else                    Log.err("{0}::Exception: {1}",classType.Name, e.Message);
            }
        }

        void                            ITestFrameWork.setExpectedFailure   (TestForFailure toTest) {
            testForFailure = toTest;
        }
    }

    internal interface                  ITestFrameWork                      {
        internal void                   setExpectedFailure                  (TestForFailure toTest);
    }

    internal class                      FakeFile {
        private HitCheck[] map;
        long len;
        int remain;
        int chunkSize;
        public FakeFile(FileInfo fileInf, int chunkSz)
        {
            chunkSize = chunkSz;
            len = fileInf.Length;
            remain = ( int )( len % chunkSz );
            long chk = len / chunkSz + ( remain == 0 ? 0 : 1 );
            if ( chunkSz <= 0 || chk > int.MaxValue ) throw new Exception("much too small chunk size: " + chunkSz);
            map = new HitCheck[chk];
        }

        internal void assertAllProcessed()
        {
            long position = 0;
            for ( int i = 0; i < map.Length; i++ )
            {
                HitCheck currMap = map[i];
                if ( currMap == null ) throw new Exception(string.Format("missing hit at position: {0}", position));
                long mapPos = currMap.Position;
                if ( mapPos != position ) throw new Exception(string.Format("incorrect position,, expcted: {0} found: {1}", position, mapPos));
                int chunkSz = chunkSize;
                if ( remain != 0 && i == map.Length - 1 ) chunkSz = remain;
                if ( chunkSz != currMap.Size ) throw new Exception(string.Format("wrong size at position: {0} extected: {1} found: {2}", position, chunkSz, currMap.Size));
                position += chunkSize;
            }
        }

        internal void hit(ChunkDesc zzDesc)
        {
            int idx = ( int )( zzDesc.position / chunkSize );
            map[idx] = new HitCheck(zzDesc.position, zzDesc.size);
        }
        internal class HitCheck {
            public HitCheck(long position, long size)
            {
                Position = position;
                Size = size;
            }

            public long Position { get; }
            public long Size { get; }
        }
    }
}

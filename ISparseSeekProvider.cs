using System.IO;

namespace SearchSameFiles {
    public interface                    ISparseSeekProvider                 {
        public ISparseSeekHandler       getSeekHandler                      (FileInfo fileInfo);
    }
    
    public interface                    ISparseSeekHandler                  {
        public bool                     hasNext                             ();

        public ChunkDesc                next                                ();
        // TODO: use long chunk size to scale further ??
        int                             getChunkSize                        ();
        int                             getNumChunks                        ();
    }

}
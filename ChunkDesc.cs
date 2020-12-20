namespace SearchSameFiles {
    public class                        ChunkDesc                           {
        public                          ChunkDesc                           (long position, long size)                                      {
            this.position   = position;
            this.size       = size;
        }

        public long                     position                            { get; set; }
        public long                     size                                { get; set; }
    }
}

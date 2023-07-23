public struct AssemblerStackComponent
    {
        public int index;
        public int id;
        public int []entityIds;
        public int rootAssemblerEntityId;
        public int rootAssemblerId;
        public uint signType;
        public int pcId;


    
        public void SetEmpty()
        {
            this.index = 0;
            this.id = 0;
            this.entityIds = null;
            this.rootAssemblerEntityId = 0;
            this.rootAssemblerId = 0;
            this.pcId = 0;
        }




        
    }
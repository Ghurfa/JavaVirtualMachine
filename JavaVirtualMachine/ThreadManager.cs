namespace JavaVirtualMachine
{
    public static class ThreadManager
    {
        public static int ThreadAddr = 0;
        public static int GetThreadAddr()
        {
            //System.Threading.Thread thread = new System.Threading.Thread(() => { });
             
            return ThreadAddr;
        }

    }
}

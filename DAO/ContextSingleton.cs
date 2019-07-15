namespace DAO
{
    public static class ContextSingleton
    {
        private static EfContext _context;

        static ContextSingleton()
        {
            _context = new EfContext();
        }

        public static EfContext GetInstance()
        {
            return _context;
        }
    }
}

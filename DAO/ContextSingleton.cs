using System.Data.Entity;

namespace DAO
{
    public static class ContextSingleton
    {
        private static EfContext _context;

        static ContextSingleton()
        {
            _context = new EfContext();
        }

        public static void RefreshContext()
        {
            _context.Dispose();
            
            _context = new EfContext();
            
            _context.Tags.Load();
            _context.ToDoItems.Load();
            _context.Users.Load();
            _context.ItemsTags.Load();
            _context.Projects.Load();
            _context.ProjectsUsers.Load();
        }
        
        public static EfContext GetInstance()
        {
            return _context;
        }
    }
}

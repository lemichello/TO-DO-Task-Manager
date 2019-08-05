using DAO;

namespace BUS.Services
{
    public abstract class Service
    {
        public static void RefreshContext()
        {
            ContextSingleton.RefreshContext();
        }
    }
}
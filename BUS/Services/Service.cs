using DAO;

namespace BUS.Services
{
    public abstract class Service
    {
        public void Refresh()
        {
            ContextSingleton.RefreshContext();
        }

        public abstract void RefreshRepositories();
    }
}
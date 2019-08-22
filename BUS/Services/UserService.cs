using System.Linq;
using System.Windows.Forms;
using DAO.Entities;
using DAO.Repositories;

namespace BUS.Services
{
    public class UserService : Service
    {
        private readonly        IRepository<User> _repository;
        private static readonly UserService       Self;

        static UserService()
        {
            Self = new UserService();
        }

        private UserService()
        {
            _repository = new EfRepository<User>();
        }

        public void RefreshRepositories()
        {
            _repository.Refresh();
        }

        public static UserService GetInstance()
        {
            return Self;
        }

        public bool Add(string login, string password)
        {
            if (_repository.Get().Any(i => i.Login == login))
            {
                MessageBox.Show("This login already exists. Try another");
                return false;
            }

            return _repository.Add(new User
            {
                Login    = login,
                Password = password
            });
        }

        public int GetUserId(string login, string password)
        {
            return _repository.Get()
                       .FirstOrDefault(i => i.Login == login && i.Password == password)?.Id ?? -1;
        }
    }
}
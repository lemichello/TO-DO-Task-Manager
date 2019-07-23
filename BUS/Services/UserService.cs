using System.Linq;
using System.Windows.Forms;
using BUS.Models;
using DAO.Entities;
using DAO.Repositories;

namespace BUS.Services
{
    public class UserService : Service
    {
        private readonly IRepository<User> _repository;

        public UserService()
        {
            _repository = new UsersRepository();
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
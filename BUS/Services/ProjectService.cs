using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BUS.Models;
using DAO.Entities;
using DAO.Repositories;

namespace BUS.Services
{
    public class ProjectService : Service
    {
        private readonly IRepository<User>          _userRepository;
        private readonly IRepository<Project>       _projectRepository;
        private readonly IRepository<ProjectsUsers> _projectUserRepository;
        private readonly IRepository<ToDoItem>      _itemRepository;
        private readonly int                        _userId;

        public ProjectService(int userId)
        {
            _userRepository        = new UsersRepository();
            _projectRepository     = new ProjectsRepository();
            _projectUserRepository = new ProjectsUsersRepository();
            _itemRepository        = new ToDoItemsRepository();
            _userId                = userId;
        }

        public override void RefreshRepositories()
        {
            _itemRepository.Refresh();
            _userRepository.Refresh();
            _projectRepository.Refresh();
            _projectUserRepository.Refresh();
        }

        public int AddProject(ProjectModel project, IEnumerable<string> userLogins)
        {
            var users  = _userRepository.Get().ToList();
            var logins = userLogins.ToList();

            if (!logins.All(i => users.Any(user => user.Login == i)))
            {
                MessageBox.Show("One of user logins doesn't exist");
                return -1;
            }

            if (logins.Contains(users.First(user => user.Id == _userId).Login))
            {
                MessageBox.Show("You can't invite yourself. This will be done automatically");
                return -1;
            }

            var newProject = new Project {Name = project.Name};

            if (!_projectRepository.Add(newProject))
            {
                return -1;
            }

            // Adding user, which created project.
            _projectUserRepository.Add(new ProjectsUsers
            {
                ProjectId  = newProject.Id,
                InviterId  = _userId,
                UserId     = _userId,
                IsAccepted = true
            });

            return AddInvitedUsers(logins, newProject, users, newProject.Id) ? newProject.Id : -1;
        }

        private bool AddInvitedUsers(IEnumerable<string> logins, Project newProject, List<User> users, int projectId)
        {
            var allProjects = _projectUserRepository.Get().ToList();
            
            foreach (var login in logins)
            {
                // This user is already invited.
                if (allProjects.Any(i => i.UserOf.Login == login && i.ProjectId == projectId))
                {
                    continue;
                }
                
                if (!_projectUserRepository.Add(new ProjectsUsers
                {
                    ProjectId  = newProject.Id,
                    InviterId  = _userId,
                    UserId     = users.Find(user => user.Login == login).Id,
                    IsAccepted = false
                }))
                {
                    return false;
                }
            }

            return true;
        }

        public bool AcceptInvitation(InvitationRequestModel invitation)
        {
            var invite = _projectUserRepository.Get().ToList().FirstOrDefault(i =>
                i.ProjectId == invitation.ProjectId &&
                i.UserId == _userId);

            if (invite == null)
                return false;

            invite.IsAccepted = true;

            _projectUserRepository.SaveChanges();

            return true;
        }

        public bool DeclineInvitation(InvitationRequestModel invitation)
        {
            var invite = _projectUserRepository.Get().First(i =>
                i.ProjectId == invitation.ProjectId &&
                i.UserId == _userId);

            return _projectUserRepository.Remove(invite);
        }

        public void LeaveProject(int projectId)
        {
            var record = _projectUserRepository.Get().ToList()
                .First(i => i.ProjectId == projectId && i.UserId == _userId);

            _projectUserRepository.Remove(record);

            // Project hasn't users.
            if (_projectUserRepository.Get().ToList().All(i => i.ProjectId != projectId))
            {
                // Deleting all items, that belong to this project.
                var projectItems = _itemRepository.Get().ToList().Where(i => i.ProjectId == projectId).ToList();

                foreach (var i in projectItems)
                {
                    _itemRepository.Remove(i);
                }
                
                _projectRepository.Remove(_projectRepository.Get().ToList().First(i => i.Id == projectId));
            }
        }
        
        public IEnumerable<ProjectModel> GetProjects()
        {
            return _projectUserRepository.Get().ToList().Where(i => i.UserId == _userId && i.IsAccepted).Select(
                i => new ProjectModel
                {
                    Id   = i.ProjectId,
                    Name = i.ProjectOf.Name
                });
        }

        public IEnumerable<InvitationRequestModel> GetInvitations()
        {
            return _projectUserRepository.Get().ToList().Where(i => i.UserId == _userId && !i.IsAccepted).Select(i =>
                new InvitationRequestModel
                {
                    InviterName = i.InviterOf.Login,
                    ProjectId   = i.ProjectId,
                    ProjectName = i.ProjectOf.Name
                }).ToList();
        }
    }
}
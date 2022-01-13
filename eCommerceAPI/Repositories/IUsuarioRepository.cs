using eCommerceAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eCommerceAPI.Repositories
{
    interface IUsuarioRepository
    {
        public List<Usuario> Get();
        public Usuario Get(int id);
        public void Create(Usuario usuario);
        public void Update(Usuario usuario);
        public void Delete(int id);
    }
}

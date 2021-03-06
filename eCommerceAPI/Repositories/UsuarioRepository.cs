using eCommerceAPI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;

namespace eCommerceAPI.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private IDbConnection _connection;
        public UsuarioRepository()
        {
            _connection = new SqlConnection(@"Data Source=DESKTOP-DD8VGPR\SQLEXPRESS;Initial Catalog=EcommerceDapper;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        }

        public List<Usuario> Get()
        {
            return _connection.Query<Usuario>("SELECT * FROM Usuarios").ToList();
        }

        public Usuario Get(int id)
        {
            return _connection.Query<Usuario, Contato, Usuario>(
                "SELECT * FROM Usuarios AS U LEFT JOIN Contatos AS C ON U.Id = C.UsuarioId WHERE U.Id = @Id",
                (usuario, contato) =>
                {
                    usuario.Contato = contato;
                    return usuario;
                },
                new { Id = id }
                ).SingleOrDefault();
        }

        public void Create(Usuario usuario)
        {
            _connection.Open();
            var transaction = _connection.BeginTransaction();
            try
            {
                string sql = "INSERT INTO Usuarios(Nome, Email, Sexo, RG, CPF, NomeMae, SituacaoCadastro, DataCadastro)" +
               "VALUES(@Nome, @Email, @Sexo, @RG, @CPF, @NomeMae, @SituacaoCadastro, @DataCadastro); " +
               "SELECT CAST( SCOPE_IDENTITY() AS INT );";
                usuario.Id = _connection.Query<int>(sql, usuario, transaction).Single();

                if (usuario.Contato != null)
                {
                    usuario.Contato.UsuarioId = usuario.Id;
                    string sqlContato = "INSERT INTO Contatos(UsuarioId, Telefone, Celular) VALUES(@UsuarioId, @Telefone, @Celular); SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    usuario.Contato.Id = _connection.Query<int>(sqlContato, usuario.Contato, transaction).Single();
                }

                transaction.Commit();
            }
            catch (Exception)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception)
                {

                }
            }
            finally
            {
                _connection.Close();
            }      
        }

        public void Update(Usuario usuario)
        {
            _connection.Open();
            var transaction = _connection.BeginTransaction();

            try
            {
                string sql = "UPDATE Usuarios SET Nome = @Nome, Email = @Email, Sexo = @Sexo, RG = @RG, CPF = @CPF, NomeMae = @NomeMae, " +
               "SituacaoCadastro = @SituacaoCadastro, DataCadastro = @DataCadastro WHERE Id = @Id";
                _connection.Execute(sql, usuario, transaction);

                if (usuario.Contato != null)
                {
                    string sqlContato = "UPDATE Contatos SET UsuarioId = @UsuarioId, Telefone = @Telefone, Celular = @Celular WHERE Id = @Id;";
                    _connection.Execute(sqlContato, usuario.Contato, transaction);
                }
                
                transaction.Commit();
            }
            catch(Exception ex)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception)
                {

                }
            }
            finally
            {
                _connection.Close();
            }
        }

        public void Delete(int id)
        {
            _connection.Execute("DELETE FROM Usuarios WHERE Id = @Id", new { Id = id });
        }
    }
}

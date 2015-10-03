using System;
using Case42.Server.ValueObjects;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode;


namespace Case42.Server.Entities
{
    public class User
    {
        public virtual uint Id { get; set; }
        public virtual string Email { get; set; }
        public virtual string Username { get; set; }
        public virtual DateTime CreatedAt { get; set; }

        public virtual HashedPassword Password { get; set; }
    }

    public class Usermap : ClassMapping<User>
    {
        public Usermap()
        {
            Table("users"); //set the table to users

            Id(x => x.Id, x => x.Generator(Generators.Identity));

            Property(x => x.Email, x => x.NotNullable(true)); //the database column is Email
            Property(x => x.Username, x => x.NotNullable(true)); //the database column is Username

            Component(x => x.Password, y =>
            {
                y.Property(x => x.Salt, z =>
                {
                    z.Column("password_salt"); //follow the naming convention at database
                    z.NotNullable(true);
                }
                );

                y.Property(x => x.Hash, z =>
                {
                    z.Column("password_hash"); //follow the naming convention at database
                    z.NotNullable(true);
                }
                );
            }
            );

            Property(x => x.CreatedAt, x =>
            {
                x.Column("created_at"); //follow the naming convention at database
                x.NotNullable(true);
            }
            );
        }
    }
}

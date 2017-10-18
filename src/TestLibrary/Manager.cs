using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestLibrary
{
    public class Manager
    {
        public Manager()
        {
            DoWorkLarge();
        }

        public void DoWorkLarge()
        {
            List<Person> list = new List<Person>();
            var person = new Person(1, "P" + 1.ToString());
            person.Name = "name2";
            list.Add(person);

            person.Work();
            person.WorkVirtual();
            person.Save();
            string s = person.ToString();
            //foreach (int i in Enumerable.Range(0, 100))
            //{
            //    list.Add(new Person(i, "P" + i.ToString()));
            //}
        }
    }

    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public Person(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public void Work()
        {
            Id++;
        }

        public virtual void WorkVirtual()
        {
            Id--;
        }

        public void Save()
        {
            try
            {
                Id++;
            }
            catch (Exception ex)
            {
                ex.ToString();
                throw;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

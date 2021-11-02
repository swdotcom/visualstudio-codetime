namespace CodeTime
{
    public class RepoMember
    {
        public string name { get; set; }
        public string email { get; set; }
        public string identifier { get; set; }
        public RepoMember(string name, string email)
        {
            this.name = name;
            this.email = email;
        }
    }
}

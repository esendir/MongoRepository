## MongoRepository
Repository pattern applied to Mongo C# Driver

### Usage

#### Model
You don't need to create a model, but if you are doing so you need to extend Entity
```csharp
	//if you are able to define you model
	public class User : Entity
	{
		public string Username { get; set; }
		public string Password { get; set; }
	}
```

#### Repository
There are multiple base constructors, read summary for your own usage
```csharp
	public class UserRepository : Repository<User>
	{
	    public UserRepository(string connectionString) : base(connectionString)
	    { }
	    
		//custom method
		public User FindbyUsername(string username)
		{
			return First(i => i.Username == username);
		}
	}
```

*If you want to create a repository for already defined model*
```csharp
	public class UserRepository : Repository<Entity<User>>
	{
	    public UserRepository(string connectionString) : base(connectionString)
	    { }
	    
		//custom method
		public User FindbyUsername(string username)
		{
			return First(i => i.Content.Username == username);
		}
	}
```

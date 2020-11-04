[![Version](https://img.shields.io/nuget/v/Repository.Mongo.svg?style=flat-square)](https://www.nuget.org/packages/Repository.Mongo)
[![Downloads](https://img.shields.io/nuget/dt/Repository.Mongo.svg?style=flat-square)](https://www.nuget.org/packages/Repository.Mongo)

## MongoRepository
Repository pattern for MongoDB with extended features

### MongoDB Driver Version
2.11.4

### Definition

#### Model
You don't need to create a model, but if you are doing so you need to extend Entity
```csharp
// If you are able to define your model
public class User : Entity 
{
    public string Username { get; set; }
    public string Password { get; set; }
}	
```

#### Repository
There are multiple base constructors, read summaries of others
```csharp
public class UserRepository : Repository<User> 
{
    public UserRepository (string connectionString) : base (connectionString) { }

    // Custom method
    public User FindbyUsername (string username) 
    {
        return First (i => i.Username == username);
    }

    // Custom method2
    public void UpdatePassword (User item, string newPassword) 
    {
        repo.Update (item, i => i.Password, newPassword);
    }

    // Custom async method
    public async Task<User> FindbyUsernameAsync (string username) 
    {
        return await FirstAsync (i => i.Username == username);
    }
}
```

*If you want to create a repository for already defined non-entity model*
```csharp
public class UserRepository : Repository<Entity<User>> 
{
    public UserRepository (string connectionString) : base (connectionString) { }


    // Custom method
    public User FindbyUsername (string username) 
    {
        return First (i => i.Content.Username == username);
    }
}	
```

### Usage

Each method has multiple overloads, read method summary for additional parameters

```csharp
UserRepository repo = new UserRepository ("mongodb://localhost/sample")

// Get
User user = repo.Get ("58a18d16bc1e253bb80a67c9");

// Insert
User item = new User () 
{
    Username = "username",
    Password = "password"
};
repo.Insert (item);

// Update
// Single property
repo.Update (item, i => i.Username, "newUsername");

// Multiple property
// Updater has many methods like Inc, Push, CurrentDate, etc.
var update1 = Updater.Set (i => i.Username, "oldUsername");
var update2 = Updater.Set (i => i.Password, "newPassword");
repo.Update (item, update1, update2);

// All entity
item.Username = "someUsername";
repo.Replace (item);

// Delete
repo.Delete (item);

// Queries - all queries has filter, order and paging features
var first = repo.First ();
var last = repo.Last ();
var search = repo.Find (i => i.Username == "username");
var allItems = repo.FindAll ();

// Utils
var any = repo.Any (i => i.Username.Contains ("user"));

// Count
// Get number of filtered documents
var count = repo.Count (p => p.Age > 20);

// EstimatedCount
// Get number of all documents
var count = repo.EstimatedCount ();
```

### List of Functions
```csharp
Delete(T entity)
Task<bool> DeleteAsync(T entity)

Delete(Expression<Func<T, bool>> filter)
Task<bool> DeleteAsync(Expression<Func<T, bool>> filter)

IEnumerable<T> Find(Expression<Func<T, bool>> filter, int pageIndex, int size)
IEnumerable<T> Find(Expression<Func<T, bool>> filter, Expression<Func<T, object>> order, int pageIndex, int size)

IEnumerable<T> FindAll(int pageIndex, int size)
IEnumerable<T> FindAll(Expression<Func<T, object>> order, int pageIndex, int size)

T First()
T First(FilterDefinition<T> filter)
T First(Expression<Func<T, bool>> filter)
T First(Expression<Func<T, bool>> filter, Expression<Func<T, object>> order)
T First(Expression<Func<T, bool>> filter, Expression<Func<T, object>> order, bool isDescending)

Task<T> FirstAsync(FilterDefinition<T> filter)
Task<T> FirstAsync(Expression<Func<T, bool>> filter)

T Last()
T Last(FilterDefinition<T> filter)
T Last(Expression<Func<T, bool>> filter)
T Last(Expression<Func<T, bool>> filter, Expression<Func<T, object>> order)
T Last(Expression<Func<T, bool>> filter, Expression<Func<T, object>> order, bool isDescending)

void Replace(IEnumerable<T> entities)

T FindOneAndUpdate(FilterDefinition<T> filter, UpdateDefinition<T> update, FindOneAndUpdateOptions<T> options = null)
T FindOneAndUpdate(Expression<Func<T, bool>> filter, UpdateDefinition<T> update, FindOneAndUpdateOptions<T> options = null)

Task<T> FindOneAndUpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> update, FindOneAndUpdateOptions<T> options = null)
Task<T> FindOneAndUpdateAsync(Expression<Func<T, bool>> filter, UpdateDefinition<T> update, FindOneAndUpdateOptions<T> options = null)

bool Update<TField>(T entity, Expression<Func<T, TField>> field, TField value)

Task<bool> UpdateAsync<TField>(T entity, Expression<Func<T, TField>> field, TField value)

bool Any(Expression<Func<T, bool>> filter)

bool Update<TField>(FilterDefinition<T> filter, Expression<Func<T, TField>> field, TField value)
bool Update(FilterDefinition<T> filter, params UpdateDefinition<T>[] updates)
bool Update(Expression<Func<T, bool>> filter, params UpdateDefinition<T>[] updates)

Task<bool> UpdateAsync(FilterDefinition<T> filter, params UpdateDefinition<T>[] updates)
Task<bool> UpdateAsync(Expression<Func<T, bool>> filter, params UpdateDefinition<T>[] updates)
Task<bool> UpdateAsync<TField>(FilterDefinition<T> filter, Expression<Func<T, TField>> field, TField value)

long EstimatedCount()
long Count(Expression<Func<T, bool>> filter)
long EstimatedCount(EstimatedDocumentCountOptions options)

Task<long> EstimatedCountAsync()
Task<long> CountAsync(Expression<Func<T, bool>> filter)
Task<long> EstimatedCountAsync(EstimatedDocumentCountOptions options)
```
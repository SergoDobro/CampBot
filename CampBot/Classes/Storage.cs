using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace CampBot.Classes
{
    class User
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Location { get; set; }
        public List<string> Masters { get; set; }
        public List<string> Children { get; set; }
    }
    class Place
    {

        public string Tag { get; set; }
        public string PlaceName { get; set; }
        public string WhereAreYou { get; set; }
        public string WhereIsHe { get; set; }
    }
    internal class Storage
    {
        private SQLiteConnection _connection;
        private TelegramBotClient _bot;

        public Storage(string dbName, TelegramBotClient _bot)
        {
            this._bot = _bot;
            _connection = new SQLiteConnection(dbName);
            _connection.Open();
        }
        public async Task Init()
        {
            await _connection.ExecuteAsync(
                @"
                    CREATE TABLE IF NOT EXISTS users(
                        Id INTEGER PRIMARY KEY,
                        Name TEXT,
                        UserName TEXT,
                        Location TEXT,
                        Masters JSON,
                        Children JSON
                    )
                ");
            await _connection.ExecuteAsync(
                @"
                    CREATE TABLE IF NOT EXISTS places(
                        Tag TEXT,
                        PlaceName TEXT,
                        WhereAreYou TEXT,
                        WhereIsHe TEXT
                    )
                ");
        }
        public async Task<Place> GetPlace(string tag)
        {



            var place = await _connection.QueryFirstOrDefaultAsync<Place>(
                @"
                    SELECT * FROM places WHERE Tag = @Tag
                ", new { Tag = tag });
            if (place == null)
            {
                place = new Place() { Tag = "smw", PlaceName = "Где-то", WhereAreYou= "Где-то", WhereIsHe= "Где-то" };
            }
            return place;
        }
        public List<Place> GetAllPlaces()
        {
            string stm = "SELECT * FROM places";

            using var cmd = new SQLiteCommand(stm, _connection);
            using SQLiteDataReader rdr = cmd.ExecuteReader();
            List<Place> places = new List<Place>();
            while (rdr.Read())
            {
                try
                {
                    places.Add(new Place() {
                        Tag = rdr.GetString(0),
                        PlaceName = rdr.GetString(1),
                        WhereAreYou = rdr.GetString(2),
                        WhereIsHe = rdr.GetString(3)
                    });
                }
                catch 
                {
                }
            }
            return places;
        }
        public async Task<User> GetUser(long userId)
        {
            var user = await _connection.QueryFirstOrDefaultAsync<User>(
                @"
                    SELECT * FROM users WHERE Id = @Id
                ", new { Id = userId });
            if (user == null)
            {
                user = new User() { Id = userId };
                await _connection.ExecuteAsync(
                    @"
                        INSERT INTO users (Id) VALUES (@id)
                    ", new { id = userId });
            }
            if (user.Masters == null) user.Masters = new List<string>();
            if (user.Children == null) user.Children = new List<string>();

            return user;
        }
        public async Task<User> GetUserByUserName(string userName)
        {
            var user = await _connection.QueryFirstOrDefaultAsync<User>(
                @"
                    SELECT * FROM users WHERE UserName = @UserName
                ", new { UserName = userName });

            if (user == null)
            {
                user = new User() { UserName = userName };
                await _connection.ExecuteAsync(
                    @"
                        INSERT INTO users (UserName) VALUES (@UserName)
                    ", new { UserName = userName });
                await UpdateUserId(user);
            }
            if (user.Masters == null) user.Masters = new List<string>();
            if (user.Children == null) user.Children = new List<string>();

            return user;
        }
        public async Task SetChildLocation(long childId, string location)
        {
            User child = await GetUser(childId);
            child.Location = location;
            await UpdateUserLocation(child);
            foreach (var m in child.Masters)
            {
                try
                {
                    await _bot.SendTextMessageAsync((await GetUserByUserName(m)).Id, child.Name + " теперь в " + (await GetPlace(location)).WhereAreYou);
                }
                catch
                {
                }
            }
        }
        public async Task UpdateUserName(User user)
        {
            await _connection.ExecuteAsync(
                @"
                    UPDATE users SET Name = @Name WHERE Id = @Id
                ", user);
        }
        public async Task UpdateUserId(User user)
        {
            await _connection.ExecuteAsync(
                @"
                    UPDATE users SET Id = @Id WHERE UserName = @UserName
                ", user);
        }
        public async Task UpdateUserMasters(User user)
        {
            if (user.Id != 0)
            {
                await _connection.ExecuteAsync(
                @"
                    UPDATE users SET Masters = @Masters WHERE Id = @Id
                ", user);
            }
            else
            {
                await _connection.ExecuteAsync(
                @"
                    UPDATE users SET Masters = @Masters WHERE UserName = @UserName
                ", user);
            }

        }
        public async Task UpdateUserChildren(User user)
        {
            await _connection.ExecuteAsync(
                @"
                    UPDATE users SET Children = @Children WHERE Id = @Id
                ", user);
        }
        public async Task UpdateUserLocation(User user)
        {

            await _connection.ExecuteAsync(
                @"
                    UPDATE users SET Location = @Location WHERE Id = @Id
                ", user);
        }
        public async Task AddNewUser(Telegram.Bot.Types.User newUser)
        {
            User user = await GetUserByUserName(newUser.Username);
            bool flag = false;
            if (user.Name != newUser.FirstName + " " + newUser.LastName)
            { user.Name = newUser.FirstName + " " + newUser.LastName; flag = true; }
            if (newUser.Username != null)
            {
                if (newUser.Username != user.UserName)
                {
                    user.UserName = newUser.Username;
                    flag = true;
                }
            }
            if (flag)
            {
                await UpdatesUserNames(user);
            }
            if (user.Id == 0)
            {
                user.Id = newUser.Id;
                await UpdateUserId(user);
            }
        }
        public async Task SetUserMaster(long childId, long masterId, string name)
        {
            User child = await GetUser(childId);
            User master = await GetUser(masterId);

            if (name != "")
            {
                child.Name = name;
                await UpdateUserName(child);
            }
            if (!child.Masters.Contains(master.UserName))
                child.Masters.Add(master.UserName);
            if (!master.Children.Contains(child.UserName))
                master.Children.Add(child.UserName);

            child.Masters = child.Masters.Distinct().ToList();
            master.Children = master.Children.Distinct().ToList();
            UpdateUserMasters(child);
            UpdateUserChildren(master);
        }
        public async Task<bool> SetUserMasterByUserName(long masterId, string username)
        {

            User child = await GetUserByUserName(username);
            User master = await GetUser(masterId);

            if (!child.Masters.Contains(master.UserName))
                child.Masters.Add(master.UserName);
            if (!master.Children.Contains(child.UserName))
                master.Children.Add(child.UserName);

            child.Masters = child.Masters.Distinct().ToList();
            master.Children = master.Children.Distinct().ToList();
            await UpdateUserMasters(child);
            await UpdateUserChildren(master);
            return true;
        }
        public async Task UpdatesUserNames(User user)
        {

            await _connection.ExecuteAsync(
                @"
                    UPDATE users SET Name = @Name, UserName = @UserName WHERE Id = @Id
                ", user);
        }
    }
}

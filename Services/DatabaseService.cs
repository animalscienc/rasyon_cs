using Microsoft.Data.Sqlite;
using ZootekniPro.App.Models;
using System.IO;
using System.Collections.Generic;

namespace ZootekniPro.App.Services;

public class DatabaseService
{
    private readonly string _connectionString;
    private readonly string _dbPath;

    public DatabaseService()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ZootekniPro");
        Directory.CreateDirectory(appDataPath);
        _dbPath = Path.Combine(appDataPath, "zootekni.db");
        _connectionString = $"Data Source={_dbPath}";
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Feeds (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Category TEXT NOT NULL,
                Origin TEXT,
                DM REAL,
                CP REAL,
                NDF REAL,
                ADF REAL,
                NEL REAL,
                ME REAL,
                Ca REAL,
                P REAL,
                Mg REAL,
                K REAL,
                RDP REAL,
                RUP REAL,
                NPN REAL,
                Price REAL,
                VegetationPeriod TEXT,
                Notes TEXT
            );

            CREATE TABLE IF NOT EXISTS AnimalGroups (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Species TEXT,
                BodyWeight REAL,
                MilkYield REAL,
                MilkFat REAL,
                MilkProtein REAL,
                LactationWeek INTEGER,
                Stage TEXT,
                ADG REAL,
                IsHeifer INTEGER
            );

            CREATE TABLE IF NOT EXISTS Rations (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                AnimalGroupId INTEGER,
                AnimalGroupName TEXT,
                CreatedDate TEXT,
                Version TEXT,
                TotalCost REAL,
                TotalDM REAL,
                FOREIGN KEY (AnimalGroupId) REFERENCES AnimalGroups(Id)
            );

            CREATE TABLE IF NOT EXISTS RationFeeds (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                RationId INTEGER,
                FeedId INTEGER,
                FeedName TEXT,
                Amount REAL,
                Cost REAL,
                CP REAL,
                NDF REAL,
                NEL REAL,
                FOREIGN KEY (RationId) REFERENCES Rations(Id),
                FOREIGN KEY (FeedId) REFERENCES Feeds(Id)
            );
        ";
        command.ExecuteNonQuery();
    }

    // Feed CRUD Operations
    public List<Feed> GetAllFeeds()
    {
        var feeds = new List<Feed>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Feeds ORDER BY Category, Name";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            feeds.Add(MapFeed(reader));
        }
        return feeds;
    }

    public Feed? GetFeedById(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Feeds WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapFeed(reader);
        }
        return null;
    }

    public int AddFeed(Feed feed)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Feeds (Name, Category, Origin, DM, CP, NDF, ADF, NEL, ME, Ca, P, Mg, K, RDP, RUP, NPN, Price, VegetationPeriod, Notes)
            VALUES (@Name, @Category, @Origin, @DM, @CP, @NDF, @ADF, @NEL, @ME, @Ca, @P, @Mg, @K, @RDP, @RUP, @NPN, @Price, @VegetationPeriod, @Notes);
            SELECT last_insert_rowid();";

        AddFeedParameters(command, feed);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void UpdateFeed(Feed feed)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Feeds SET 
                Name = @Name, Category = @Category, Origin = @Origin, DM = @DM, CP = @CP,
                NDF = @NDF, ADF = @ADF, NEL = @NEL, ME = @ME, Ca = @Ca, P = @P,
                Mg = @Mg, K = @K, RDP = @RDP, RUP = @RUP, NPN = @NPN, 
                Price = @Price, VegetationPeriod = @VegetationPeriod, Notes = @Notes
            WHERE Id = @Id";

        command.Parameters.AddWithValue("@Id", feed.Id);
        AddFeedParameters(command, feed);
        command.ExecuteNonQuery();
    }

    public void DeleteFeed(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Feeds WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);
        command.ExecuteNonQuery();
    }

    // Ration CRUD Operations
    public List<Ration> GetAllRations()
    {
        var rations = new List<Ration>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Rations ORDER BY CreatedDate DESC";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            rations.Add(new Ration
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                AnimalGroupId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2),
                AnimalGroupName = reader.IsDBNull(3) ? "" : reader.GetString(3),
                CreatedDate = DateTime.Parse(reader.GetString(4)),
                Version = reader.GetString(5),
                TotalCost = reader.IsDBNull(6) ? 0 : reader.GetDouble(6),
                TotalDM = reader.IsDBNull(7) ? 0 : reader.GetDouble(7)
            });
        }
        return rations;
    }

    public int SaveRation(Ration ration)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Rations (Name, AnimalGroupId, AnimalGroupName, CreatedDate, Version, TotalCost, TotalDM)
            VALUES (@Name, @AnimalGroupId, @AnimalGroupName, @CreatedDate, @Version, @TotalCost, @TotalDM);
            SELECT last_insert_rowid();";

        command.Parameters.AddWithValue("@Name", ration.Name);
        command.Parameters.AddWithValue("@AnimalGroupId", ration.AnimalGroupId);
        command.Parameters.AddWithValue("@AnimalGroupName", ration.AnimalGroupName);
        command.Parameters.AddWithValue("@CreatedDate", ration.CreatedDate.ToString("O"));
        command.Parameters.AddWithValue("@Version", ration.Version);
        command.Parameters.AddWithValue("@TotalCost", ration.TotalCost);
        command.Parameters.AddWithValue("@TotalDM", ration.TotalDM);

        var rationId = Convert.ToInt32(command.ExecuteScalar());

        // Save ration feeds
        foreach (var rf in ration.Feeds)
        {
            var rfCommand = connection.CreateCommand();
            rfCommand.CommandText = @"
                INSERT INTO RationFeeds (RationId, FeedId, FeedName, Amount, Cost, CP, NDF, NEL)
                VALUES (@RationId, @FeedId, @FeedName, @Amount, @Cost, @CP, @NDF, @NEL)";
            rfCommand.Parameters.AddWithValue("@RationId", rationId);
            rfCommand.Parameters.AddWithValue("@FeedId", rf.FeedId);
            rfCommand.Parameters.AddWithValue("@FeedName", rf.FeedName);
            rfCommand.Parameters.AddWithValue("@Amount", rf.Amount);
            rfCommand.Parameters.AddWithValue("@Cost", rf.Cost);
            rfCommand.Parameters.AddWithValue("@CP", rf.CP);
            rfCommand.Parameters.AddWithValue("@NDF", rf.NDF);
            rfCommand.Parameters.AddWithValue("@NEL", rf.NEL);
            rfCommand.ExecuteNonQuery();
        }

        return rationId;
    }

    public void DeleteRation(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM RationFeeds WHERE RationId = @Id; DELETE FROM Rations WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id);
        command.ExecuteNonQuery();
    }

    // Animal Group Operations
    public List<AnimalGroup> GetAllAnimalGroups()
    {
        var groups = new List<AnimalGroup>();
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM AnimalGroups ORDER BY Name";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            groups.Add(new AnimalGroup
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Species = reader.IsDBNull(2) ? "Sığır" : reader.GetString(2),
                BodyWeight = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
                MilkYield = reader.IsDBNull(4) ? 0 : reader.GetDouble(4),
                MilkFat = reader.IsDBNull(5) ? 0 : reader.GetDouble(5),
                MilkProtein = reader.IsDBNull(6) ? 0 : reader.GetDouble(6),
                LactationWeek = reader.IsDBNull(7) ? 1 : reader.GetInt32(7),
                Stage = reader.IsDBNull(8) ? "Laktasyon" : reader.GetString(8),
                ADG = reader.IsDBNull(9) ? 0 : reader.GetDouble(9),
                IsHeifer = reader.IsDBNull(10) ? false : reader.GetInt32(10) == 1
            });
        }
        return groups;
    }

    public int SaveAnimalGroup(AnimalGroup group)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO AnimalGroups (Name, Species, BodyWeight, MilkYield, MilkFat, MilkProtein, LactationWeek, Stage, ADG, IsHeifer)
            VALUES (@Name, @Species, @BodyWeight, @MilkYield, @MilkFat, @MilkProtein, @LactationWeek, @Stage, @ADG, @IsHeifer);
            SELECT last_insert_rowid();";

        command.Parameters.AddWithValue("@Name", group.Name);
        command.Parameters.AddWithValue("@Species", group.Species);
        command.Parameters.AddWithValue("@BodyWeight", group.BodyWeight);
        command.Parameters.AddWithValue("@MilkYield", group.MilkYield);
        command.Parameters.AddWithValue("@MilkFat", group.MilkFat);
        command.Parameters.AddWithValue("@MilkProtein", group.MilkProtein);
        command.Parameters.AddWithValue("@LactationWeek", group.LactationWeek);
        command.Parameters.AddWithValue("@Stage", group.Stage);
        command.Parameters.AddWithValue("@ADG", group.ADG);
        command.Parameters.AddWithValue("@IsHeifer", group.IsHeifer ? 1 : 0);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    private Feed MapFeed(SqliteDataReader reader)
    {
        return new Feed
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Category = reader.GetString(2),
            Origin = reader.IsDBNull(3) ? "" : reader.GetString(3),
            DM = reader.IsDBNull(4) ? 0 : reader.GetDouble(4),
            CP = reader.IsDBNull(5) ? 0 : reader.GetDouble(5),
            NDF = reader.IsDBNull(6) ? 0 : reader.GetDouble(6),
            ADF = reader.IsDBNull(7) ? 0 : reader.GetDouble(7),
            NEL = reader.IsDBNull(8) ? 0 : reader.GetDouble(8),
            ME = reader.IsDBNull(9) ? 0 : reader.GetDouble(9),
            Ca = reader.IsDBNull(10) ? 0 : reader.GetDouble(10),
            P = reader.IsDBNull(11) ? 0 : reader.GetDouble(11),
            Mg = reader.IsDBNull(12) ? 0 : reader.GetDouble(12),
            K = reader.IsDBNull(13) ? 0 : reader.GetDouble(13),
            RDP = reader.IsDBNull(14) ? 0 : reader.GetDouble(14),
            RUP = reader.IsDBNull(15) ? 0 : reader.GetDouble(15),
            NPN = reader.IsDBNull(16) ? 0 : reader.GetDouble(16),
            Price = reader.IsDBNull(17) ? 0 : reader.GetDouble(17),
            VegetationPeriod = reader.IsDBNull(18) ? "" : reader.GetString(18),
            Notes = reader.IsDBNull(19) ? "" : reader.GetString(19)
        };
    }

    private void AddFeedParameters(SqliteCommand command, Feed feed)
    {
        command.Parameters.AddWithValue("@Name", feed.Name);
        command.Parameters.AddWithValue("@Category", feed.Category);
        command.Parameters.AddWithValue("@Origin", feed.Origin);
        command.Parameters.AddWithValue("@DM", feed.DM);
        command.Parameters.AddWithValue("@CP", feed.CP);
        command.Parameters.AddWithValue("@NDF", feed.NDF);
        command.Parameters.AddWithValue("@ADF", feed.ADF);
        command.Parameters.AddWithValue("@NEL", feed.NEL);
        command.Parameters.AddWithValue("@ME", feed.ME);
        command.Parameters.AddWithValue("@Ca", feed.Ca);
        command.Parameters.AddWithValue("@P", feed.P);
        command.Parameters.AddWithValue("@Mg", feed.Mg);
        command.Parameters.AddWithValue("@K", feed.K);
        command.Parameters.AddWithValue("@RDP", feed.RDP);
        command.Parameters.AddWithValue("@RUP", feed.RUP);
        command.Parameters.AddWithValue("@NPN", feed.NPN);
        command.Parameters.AddWithValue("@Price", feed.Price);
        command.Parameters.AddWithValue("@VegetationPeriod", feed.VegetationPeriod);
        command.Parameters.AddWithValue("@Notes", feed.Notes);
    }
}
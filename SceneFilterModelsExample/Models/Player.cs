using System;
using System.Collections.Generic;

namespace SceneFilterModelsExample.Models;

public class User
{
    public long Uid { get; set; }
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public Address Address { get; set; }
    public bool IsMale { get; set; }
}

public class Player
{
    public User User { get; set; }
    public long Pid { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public Version GameVersion { get; set; }
    public List<string> Pets { get; set; }
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Country { get; set; }
}
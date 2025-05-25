﻿namespace MauiApp.Models;

public class TestForCheck
{
    public int UserId { get; set; }
    public int TestId { get; set; }
    
    public List<UserAnswer> Answers { get; set; }
}

public class UserAnswer
{
    public int TaskId { get; set; }
    public string Answer { get; set; }
}
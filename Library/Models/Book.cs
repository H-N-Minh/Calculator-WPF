using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace Library.Models;

public class Book : INotifyPropertyChanged
{
	private string? title;

	public string? Title
	{
		get { return title; }
		set { title = value; OnPropertyChanged(); }
    }
	private string? author;

	public string? Author
	{
		get { return author; }
		set { author = value; OnPropertyChanged(); }
    }
	private DateTime? publishDate;

	public DateTime? PublishDate
	{
		get { return publishDate; }
		set { publishDate = value; OnPropertyChanged(); }
    }
	private int? pages;

	public int? Pages
	{
		get { return pages; }
		set { pages = value; OnPropertyChanged(); }
    }
	private decimal? price;

	public decimal? Price
	{
		get { return price; }
		set { price = value; OnPropertyChanged(); }
    }

	private float? rating;

	public float? Rating
	{
		get { return rating; }
		set { rating = value; OnPropertyChanged(); }
    }
	private string? description;
    public string? Description
	{
		get { return description; }
		set { description = value; OnPropertyChanged(); }
	}

	public Book(string title, string? author = null, DateTime? publishDate = null, int? pages = null, 
				decimal? price = null, float? rating = null, string? description = null)
	{
		Title = title;
		Author = author;
		PublishDate = publishDate;
		Pages = pages;
		Price = price;
		Rating = rating;
		Description = description;
	}

    public event PropertyChangedEventHandler? PropertyChanged;
	public void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
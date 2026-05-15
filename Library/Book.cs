using System;
using System.Collections.Generic;
using System.Text;

namespace YT;

public class Book
{
	private string author;

	public string Author
	{
		get { return author; }
		set { author = value; }
	}

	private string title;

	public string Title
	{
		get { return title; }
		set { title = value; }
	}

	private int pageCount;

	public int PageCount
	{
		get { return pageCount; }
		set { pageCount = value; }
	}

	
}

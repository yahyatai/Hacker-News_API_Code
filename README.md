# Hacker-News
---------------------------------------------------------------- Dependency  ------------------------------------------

1.) Should have installed ASP.NET 8

---------------------------------------------------------------- How to Run  ------------------------------------------

There are multiple ways, few of them are:

1.) Clone this project and Run from Visual Studio
2.) Run from .NET Command **(dotnet run --project MyProject/MyProject.csproj)** and hit APIs from Postman or Curl

This project is calling Hacker News API which will return Best n Stories based on score in desending order.

This project is created on ASP.NET 8.0 

----------------------------------------------------------------   My assumptions  ------------------------------------------

In the required response commentCount were asked, which will be the count of kids for each story.

for example the first story id:12345, the kids array have 6 elements, so comments count could be 6. 

But I consider that kids inside each kid should also be consider in comments count for that story.

This consideration of mine creates recurrsion and thats how I calculate number of comments for each story

---------------------------------------------------------------  Current Enhancements  --------------------------------------

As the HackerNews API does not give us data in a bulk, So for each time we need to hit API with specific story Id (https://hacker-news.firebaseio.com/v0/item/{xyz}.json).

In this scenario I implement caching that if the id repeat itself no need to call to API but get data from caching. 

Although I do consider that, if that case happen my code with stuck in infinite and which means there is issue in data.


---------------------------------------------------------------  Future Enhancements  --------------------------------------

I can add multi threading, to call API multiple times at parallel

# News streaming demo

This repository contains a demo to show you how you can combine streaming 
data on Kafka with ElasticSearch percolator index to create a streaming
news subscription application.

## System requirements

You'll need the following tools to run the demo:

* .NET 5
* Docker

## Getting started

To start the demo, execute the following command:

```
docker-compose -f ./compose-elastic.yml -f ./compose-kafka.yml -f ./compose-app.yml up
```

After all the containers are running you can open your browser and navigate to
http://localhost:4200 to view and create streaming subscriptions for news items. 

To get news into the API, you can use the generator console application.
Use the following command to generate a news item:

```
dotnet run src/Generator/Generator.csproj -- --title <some-title> --body <some-body> --tags a,b,c 
```

The news item will be put on the Kafka topic for news items and 
streamed to the correct subscription in the frontend.

## How it works

### Creating a news subscription

When you create a news subscription in the frontend the application will create
a search query for news items your interested in and start a new SignalR connection
for streaming news. 

The API stores the search query in the percolator index together with the ID of
the SignalR connection that the frontend uses for streaming data from the server.

### Streaming news items

When news items are published on to Kafka, they are picked up by the API. 
The API sends the news item to the percolator index to find which query would
match the news item. The ID of the matching query corresponds with the 
SignalR connection that the client created, so we can find it and send the 
news item to that connection.

News items that are received by the API are stored in a news item index so
we can search through them at a later date should we encounter a client
that came in later and didn't see the news items that we streamed earlier.

### Handling the initial set of news items

When a client first opens the connection to the news API it will not see any
news items, because we haven't streamed anything yet. To solve this, we 
take the query that was created for the frontend and run it against the news
item index that contains the news items we received before. The items that
match the query are returned to the client so it has an initial set of news 
items.
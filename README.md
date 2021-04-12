# News streaming demo

This repository contains a demo to show you how you can combine streaming 
data on Kafka with ElasticSearch percolator index to create a streaming
news subscription application.

## System requirements

You'll need the following tools to run the demo:

* .NET 5
* Docker

## Getting started

To start the demo, first you need to start and configure the Kafka broker:

```
docker-compose start zookeeper
docker-compose start broker
```
Use the following command to create the topics:

```
docker-compose exec broker kafka-topics --create --topic raw-newsitems --bootstrap-server broker:9092 --partitions 1 --replication-factor 1
docker-compose exec broker kafka-topics --create --topic enriched-newsitems --bootstrap-server broker:9092 --partitions 1 --replication-factor 1
```

After creating the topic, you can start the rest of the services using the following command:

```
docker-compose up -d
```

## How it works

### Creating a news subscription

When you create a news subscription in the frontend the application will create
a search query for news items your interested in and start a new SignalR connection
for streaming news. 

The API stores the search query in the percolator index together with the ID of
the SignalR connection that the frontend uses for streaming data from the server.

### Streaming news items

When news items are published to Kafka, they are picked up by the enricher.

The enricher adds additional details to the news item. In this case, we've opted
for a basic sentiment score using the Text Analytics service in Azure.

After the item is enriched, it's sent to the API. The API sends the news item to 
the percolator index to find which query would match the news item. The ID of 
the matching query corresponds with the SignalR connection that the client 
created, so we can find it and send the news item to that connection.

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

## Project structure

```
└───src
    ├───Api         # Contains the source code for the API
    ├───Enricher    # Contains the source code for the Enricher
    ├───Generator   # Contains the source code for the News generator
    └───Messaging   # Contains generic messaging components
```


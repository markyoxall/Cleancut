# Local Development Containers

This document describes the local Docker containers used by the project (RabbitMQ, Redis, RedisInsight, MailHog) and how they are configured.

## Containers and commands

RabbitMQ
```
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:latest
```
- Ports:
  - `5672` - AMQP protocol (application uses this for publishing/subscribing)
  - `15672` - Management UI (web)
- Default credentials: `guest` / `guest` (works when accessing from localhost)
- Management UI: http://localhost:15672

Redis
```
docker run -d --name redis -p 6379:6379 redis:latest
```
- Ports:
  - `6379` - Redis server
- Connection string example: `localhost:6379`
- Used as the retry queue backend (when configured)

RedisInsight
```
docker run -d --name redisinsight -p 5540:5540 redis/redisinsight:latest
```
- Port `5540` - RedisInsight web UI
- UI: http://localhost:5540

MailHog
```
docker run -d --name mailhog -p 1025:1025 -p 8025:8025 mailhog/mailhog:latest
```
- Ports:
  - `1025` - SMTP (applications can send mail to this port)
  - `8025` - MailHog Web UI
- UI: http://localhost:8025

## How the app uses these services
- RabbitMQ: publisher/consumer for `OrderCreated` events. App configuration keys (see appsettings): `RabbitMq:Hostname`, `RabbitMq:Port`, `RabbitMq:Username`, `RabbitMq:Password`, `RabbitMq:Exchange`.
- Redis: optional durable retry queue. Configure via `Redis:ConnectionString` (e.g. `localhost:6379`). The retry queue key default is `cleancut:retry:orders`.
- MailHog: used as an SMTP server during development. Configure SMTP host/port to `localhost:1025` and use the web UI to inspect sent messages.

## Notes and best-practices
- These containers are for local development only. Do not expose management UIs in production.
- For Redis-backed retry queue deduplication we store a set (key: `{RetryQueueKey}:ids`) — ensure Redis persistence or durability if you rely on it between restarts.
- If you need to persist data across Docker restarts, run containers with named volumes (bind mounts) rather than ephemeral containers.

## Stopping and removing

Stop and remove containers:
```
docker stop rabbitmq redis redisinsight mailhog
docker rm rabbitmq redis redisinsight mailhog
```

## Troubleshooting
- If the app reports it cannot connect to RabbitMQ/Redis, confirm the containers are running and ports are not blocked:
  - `docker ps` to list running containers
  - `docker logs <container>` for logs
- Confirm the application is pointed to the correct host/port via its configuration (appsettings or environment variables).

If you want, I can produce a docker-compose file to start all of these services together and wire recommended volumes and environment variables.  

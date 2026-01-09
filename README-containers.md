# Local Development Containers

This document describes the local Docker containers used by the project (RabbitMQ, Redis, RedisInsight, MailHog) and how they are configured.

## Aspire Orchestration (Recommended)

The project uses **Aspire** to automatically manage all development containers. When you run the `CleanCut.AspireAppHost` project, it will automatically start and configure all required containers with persistent storage.

### Containers Managed by Aspire

- **RabbitMQ** - Message broker with management UI
- **Redis** - Cache and retry queue backend
- **RedisInsight** - Redis GUI management tool
- **MailHog** - Email testing tool

### Persistent Storage

All containers use Docker volumes to persist data between restarts:

| Container | Volume Name | Purpose |
|-----------|-------------|---------|
| RabbitMQ | `rabbitmq-data` | Queues, messages, exchanges, bindings, users |
| Redis | `redis-data` | All cached data and retry queue items |
| RedisInsight | `redis-insight-data` | EULA acceptance, saved connections, preferences |
| MailHog | `mailhog-data` | Captured email messages |

### Managing Persistent Data

**View all volumes:**
```powershell
docker volume ls
```

**Clean up all persistent data** (reset to fresh state):
```powershell
docker volume rm rabbitmq-data redis-data redis-insight-data mailhog-data
```

**Clean up individual volumes:**
```powershell
# Reset just RabbitMQ data
docker volume rm rabbitmq-data

# Reset just Redis data
docker volume rm redis-data

# Reset just RedisInsight settings
docker volume rm redis-insight-data

# Reset just MailHog emails
docker volume rm mailhog-data
```

**Note:** Containers must be stopped before removing volumes. Stop Aspire or use:
```powershell
docker stop rabbitmq redis redis-insight mailhog
```

---

## Manual Container Setup (Alternative)

If you're not using Aspire, you can run containers manually with the following commands.

### Containers and commands

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

### RabbitMQ
- **Purpose**: Publisher/consumer for `OrderCreated` events
- **Configuration keys** (appsettings): 
  - `RabbitMq:Hostname`
  - `RabbitMq:Port`
  - `RabbitMq:Username`
  - `RabbitMq:Password`
  - `RabbitMq:Exchange`
- **Aspire default**: `localhost:5672` (guest/guest)

### Redis
- **Purpose**: Optional durable retry queue and caching
- **Configuration key**: `ConnectionStrings:Redis` (e.g., `localhost:6379`)
- **Retry queue key default**: `cleancut:retry:orders`
- **Aspire default**: `localhost:6379` (no password)

### RedisInsight
- **Purpose**: GUI for browsing Redis data
- **Connection**: Use host `redis` and port `6379` when adding database in RedisInsight UI
- **Note**: RedisInsight runs in a container, so it uses the container name `redis` not `localhost`

### MailHog
- **Purpose**: SMTP server for development email testing
- **Configuration**: Set SMTP host/port to `localhost:1025`
- **Web UI**: View captured emails at http://localhost:8025

---

## Notes and best-practices

- These containers are for local development only. Do not expose management UIs in production.
- For Redis-backed retry queue deduplication we store a set (key: `{RetryQueueKey}:ids`) — ensure Redis persistence or durability if you rely on it between restarts.
- **Aspire automatically configures persistent storage** for all containers using Docker volumes.
- When using Aspire, connection strings use standard ports (no dynamic ports, no SSL, no auto-generated passwords).

---

## Stopping and removing

### When Using Aspire
Simply stop the Aspire AppHost project. Containers will stop automatically.

### Manual Container Management
Stop and remove containers:
```powershell
docker stop rabbitmq redis redisinsight mailhog
docker rm rabbitmq redis redisinsight mailhog
```

---

## Troubleshooting

### Aspire Setup
- **Redis connection fails**: Check that Redis container is running on port 6379
  - Aspire dashboard should show Redis with endpoint `tcp://localhost:6379`
  - Check logs: `info: Connected to Redis for retry queue at Unspecified/localhost:6379`

- **RedisInsight can't connect**: Use container name `redis` (not `localhost`) when adding database
  - Host: `redis`
  - Port: `6379`
  - No username/password

- **RabbitMQ connection fails**: Verify RabbitMQ is running on port 5672
  - Management UI: http://localhost:15672 (guest/guest)

### General Troubleshooting
- Confirm containers are running: `docker ps`
- Check container logs: `docker logs <container-name>`
- Verify ports are not blocked or used by other applications
- Confirm application configuration (appsettings.json or environment variables) points to correct host/port

### Volume Issues
If you encounter data corruption or want a clean slate:
```powershell
# Stop Aspire, then remove volumes
docker volume rm rabbitmq-data redis-data redis-insight-data mailhog-data
```

---

If you want, I can produce a docker-compose file to start all of these services together and wire recommended volumes and environment variables.  

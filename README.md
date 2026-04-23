# CommentsApp

Полноценное веб-приложение комментариев с древовидными ответами, вложениями, капчей, realtime-обновлениями, полнотекстовым поиском и кешированием.

## Что внутри проекта

- `CommentsApp.API` - ASP.NET Core Web API + GraphQL + SignalR.
- `CommentsApp.Application` - доменная логика, DTO, интерфейсы, валидация.
- `CommentsApp.Infrastructure` - EF Core, Redis cache, RabbitMQ, Elasticsearch, работа с файлами.
- `comments-frontend` - React/Vite frontend.
- `docker-compose.yml` - единый запуск всех сервисов.
- `db-schema.sql` - актуальная SQL Server схема базы данных.
- `db-schema-mysql.sql` - MySQL-схема для открытия/сравнения в MySQL Workbench.

## Функциональность

- Создание комментариев и ответов (вложенность без ограничений).
- Валидация полей:
  - username: латиница + цифры;
  - email: корректный формат;
  - homepage: валидный URL (необязательное поле);
  - text: обязательное.
- CAPTCHA перед отправкой.
- Загрузка файлов:
  - изображения (`jpg/png/gif`) и текстовые файлы;
  - превью для изображений;
  - открытие вложений по ссылке.
- Realtime:
  - новые комментарии прилетают через SignalR без ручного обновления страницы.
- Поиск:
  - индексация в Elasticsearch;
  - в выдаче показывается полный комментарий (включая вложения), а не только сниппет.
- Кеш:
  - страницы комментариев кешируются в Redis;
  - после нового комментария кеш инвалидируется через worker и RabbitMQ.

## Архитектура потока данных

1. Пользователь отправляет форму на frontend.
2. API валидирует входные данные и сохраняет комментарий в SQL Server.
3. API публикует событие `CommentCreated` в RabbitMQ.
4. Фоновый `CommentWorker`:
   - сбрасывает кеш комментариев в Redis;
   - индексирует комментарий в Elasticsearch.
5. API отправляет realtime-событие в SignalR Hub.
6. Frontend получает событие и обновляет список комментариев.

## Быстрый старт через Docker Compose

### Требования

- Docker Desktop (или Docker Engine + Compose plugin)
- Порты должны быть свободны:
  - `443` frontend (HTTPS)
  - `8080` frontend redirect (HTTP -> HTTPS)
  - `5000` API
  - `1433` SQL Server
  - `5672`, `15672` RabbitMQ
  - `6379` Redis
  - `9200` Elasticsearch

### Запуск

Из корня репозитория:

```bash
docker compose up -d --build
```

### Проверка

- Frontend (основной): [https://localhost](https://localhost)
- Frontend (redirect): [http://localhost:8080](http://localhost:8080)
- API Swagger (через reverse proxy): [https://localhost/swagger](https://localhost/swagger)
- GraphQL endpoint (через reverse proxy): [https://localhost/graphql](https://localhost/graphql)
- RabbitMQ UI: [http://localhost:15672](http://localhost:15672) (`guest` / `guest`)
- Elasticsearch health: [http://localhost:9200/_cluster/health](http://localhost:9200/_cluster/health)

Важно: при запуске через `docker compose` фронтенд работает на `https://localhost`.  
`http://localhost:8080` в этом режиме только делает redirect на HTTPS.

### Остановка

```bash
docker compose down
```

### Полная очистка (с удалением данных)

```bash
docker compose down -v
```

## Что поднимает compose

- `db` - SQL Server 2022
- `rabbitmq` - RabbitMQ + management UI
- `redis` - Redis 7
- `elasticsearch` - Elasticsearch 8 (single-node, security disabled for dev)
- `api` - backend ASP.NET Core (с автоприменением миграций при старте)
- `frontend` - production-сборка React на Nginx с TLS (self-signed сертификат)

## Локальный запуск без Docker

### 1) Поднять зависимости отдельно

Нужны локально доступные:
- SQL Server (`localhost:1433`)
- Redis (`localhost:6379`)
- RabbitMQ (`localhost:5672`)
- Elasticsearch (`localhost:9200`)

### 2) Backend

```bash
dotnet restore
dotnet run --project CommentsApp.API
```

API поднимется на `http://localhost:5000`.

### 3) Frontend

```bash
cd comments-frontend
npm install
npm run dev
```

Frontend поднимется на `http://localhost:5173`.

Важно: `http://localhost:5173` - это только dev-режим без Docker.  
Если запущен `docker compose`, открывайте `https://localhost`.

## Конфигурация

Основные настройки backend находятся в `CommentsApp.API/appsettings.json`.

Ключевые секции:

- `ConnectionStrings:Default` - SQL Server
- `ConnectionStrings:Redis` - Redis
- `RabbitMq:Host/User/Pass/Queue` - шина событий
- `Elastic:Uri` - адрес Elasticsearch
- `Cors:AllowedOrigins` - разрешенные origin frontend
- `FileStorage:UploadPath` - путь хранения загруженных файлов

Для docker запуска используются env-переменные из `docker-compose.yml`, которые переопределяют часть этих значений.

## Структура БД

Схема в файлах:

- `db-schema.sql` (реализованная SQL Server схема)
- `db-schema-mysql.sql` (MySQL Workbench-совместимый вариант для проверки/сравнения)

Таблицы:

- `Comments` - комментарии и ответы (self-reference через `ParentId`)
- `CommentAttachment` - вложения комментариев

Связи:

- `Comments.ParentId -> Comments.Id` (ON DELETE RESTRICT)
- `CommentAttachment.CommentId -> Comments.Id` (ON DELETE CASCADE)

## Полезные API точки

- `GET /api/comments` - список комментариев
- `POST /api/comments` - создать комментарий
- `GET /api/captcha` - получить капчу
- `GET /api/search?q=...` - поиск по комментариям
- `POST /graphql` - GraphQL запросы
- `GET /hubs/comments` - SignalR Hub

## Что улучшено в frontend

- Обновлен визуальный стиль: карточки, тени, отступы, адаптивные формы.
- Добавлен вывод email автора в карточке комментария.
- Вынесен URL API в `VITE_API_URL` для корректной работы в Docker.
- Улучшены блоки поиска, вложений и пагинации для более понятного UX.

## Типовые проблемы и решения

- **API не стартует и ошибка подключения к SQL Server**  
  Проверьте, что контейнер `db` healthy (`docker compose ps`), затем перезапустите `api`.

- **Нет результатов поиска**  
  Проверьте health Elasticsearch и логи `api`; индекс инициализируется при старте API.

- **Не вижу highlight в поиске**  
  Текущая выдача поиска показывает полный комментарий и вложения. Если нужен именно highlight (подсветка фрагмента), это отдельный режим ответа API.

- **Realtime не работает**  
  Убедитесь, что открыт `https://localhost` и в браузере нет блокировок websocket.

- **Браузер предупреждает о сертификате**  
  Для локального HTTPS в Docker используется self-signed сертификат `localhost`.  
  Предупреждение "Небезопасное подключение" в браузере в этом случае ожидаемо и нормально для dev-стенда: подтвердите исключение безопасности в браузере.

- **Не загружаются файлы**  
  Проверьте volume `uploads_data` и наличие прав на запись в контейнере `api`.

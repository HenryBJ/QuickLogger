# QuickLogger

QuickLogger is a powerful logging API designed for developers who need to store and analyze logs efficiently. It provides a scalable solution by allowing the creation of applications with unique IDs, enabling seamless log storage and retrieval. Additionally, QuickLogger dynamically extends its storage by distributing apps across multiple database providers.

## Features
- **Log Storage**: Store application logs securely with structured logging.
- **App-Based Logging**: Each log entry is associated with an application ID.
- **Scalable Storage**: Dynamically extend storage using multiple database providers.
- **Multi-Database Support**: Compatible with MySQL, MSSQL, and MongoDB.
- **Message Queue Integration**: Uses RabbitMQ for efficient log handling.
- **Flexible Querying**: Retrieve logs using sorting, pagination, and filtering.

## API Endpoints

### Admin Controller

#### Add a Database Provider
```http
POST /admin/add-bditem
```
**Request Body:**
```json
{
  "name": "mysql",
  "connectionString": "your_connection_string",
  "isSeed": true,
  "version": "1.0"
}
```
**Response:**
```json
{
  "id": "generated-db-item-id"
}
```

#### Edit a Database Provider
```http
PUT /admin/edit-bditem
```
**Request Body:**
```json
{
  "id": "db-item-id",
  "name": "mongodb",
  "active": true
}
```

#### Delete a Database Provider
```http
DELETE /admin/delete-bditem
```
**Request Body:**
```json
{
  "id": "db-item-id"
}
```

#### List Database Providers
```http
GET /admin/list-bditem
```
**Response:**
```json
{
  "items": [ { "id": "db-item-id", "name": "mysql", "active": true } ]
}
```

### App Controller

#### Create an App
```http
POST /app
```
**Request Body:**
```json
{
  "name": "My Application",
  "userId": "user-123"
}
```
**Response:**
```json
{
  "id": "generated-app-id"
}
```

#### Edit an App
```http
PUT /app
```
**Request Body:**
```json
{
  "id": "app-id",
  "name": "Updated App Name",
  "active": true
}
```

#### Delete an App
```http
DELETE /app
```
**Request Body:**
```json
{
  "id": "app-id",
  "userId": "user-123"
}
```

#### List Apps
```http
GET /app?userId=user-123
```
**Response:**
```json
{
  "items": [ { "id": "app-id", "name": "My Application", "active": true } ]
}
```

### Logger Controller

#### Log an Entry
```http
POST /logger
```
**Request Body:**
```json
{
  "appId": "app-id",
  "level": "info",
  "message": "User logged in",
  "timestamp": "2025-03-23T12:34:56Z"
}
```

#### Retrieve Logs
```http
GET /logger/logs
```
**Request Body:**
```json
{
  "appId": "app-id",
  "orderByProperty": "timestamp",
  "orderDescending": true,
  "pageNumber": 1,
  "pageSize": 50
}
```
**Response:**
```json
{
  "items": [ { "appId": "app-id", "level": "info", "message": "User logged in" } ]
}
```

## Getting Started
1. Clone this repository:
   ```sh
   git clone https://github.com/HenryBJ/QuickLogger.git
   ```
2. Navigate to the project directory:
   ```sh
   cd QuickLogger
   ```
3. Install dependencies and configure database connections.
4. Run the application:
   ```sh
   dotnet run
   ```

## License
QuickLogger is released under the MIT License.


# PopcornBytes Server

The server for the PopcornBytes app, currently work in progress.

## Local setup

- Configure the redis address in `appsettings.json`
- Add the necessary user secrets for PopcornBytes.Api project:

```json
{
  "Tmdb": {
    "BaseUrl": "https://api.themoviedb.org/3",
    "ApiKey": "<your TMDB API key>",
    "ImagesBaseUrl": "https://image.tmdb.org/t/p"
  },
  "Jwt": {
    "Issuer": "popcorn-bytes",
    "Audience": "popcorn-byters",
    "Secret": "<your JWT secret>",
    "TokenExpirationInMinutes": 720
  }
}

```

## Docker compose

Create a .env file at the solution level with the following variables:

```
TMDB_API_KEY=<your TMDB API key>
JWT_KEY=<your JWT secret>
```
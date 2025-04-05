# In progress

- [ ] Add authentication

# Planned

- [ ] Add tracking

# Backlog

- [ ] Create TMDB sessions for users at login
- [ ] Get TMDB images url from the `/configuration` endpoint at startup

# Launched

- [x] Endpoint for getting a specific season
- [x] Endpoint for getting a specific episode
- [x] Add seasons collection in TvSeries struct
- [x] Add health checks for database and TMDB
- [x] Add persistence (sqlite & migrations)
- [x] TMDB client to handle the communication with the TMDB API
- [x] Search TV series by name endpoint
- [x] Get TV series by id endpoint
  - [x] Caching
- [x] Error handling middleware
- [x] Structured logging
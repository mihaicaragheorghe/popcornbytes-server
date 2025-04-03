# In progress

- [ ] Endpoint for getting a specific season
- [ ] Endpoint for getting a specific episode

# Planned

- [ ] Add authentication
- [ ] Create TMDB sessions for users at login

- [ ] Get TMDB images url from the `/configuration` endpoint at startup

- [ ] Add tracking
- [ ] Add notifications

# Launched

- [x] Add seasons collection in TvSeries struct
- [x] Add health checks for database and TMDB
- [x] Add persistence (sqlite & migrations)
- [x] TMDB client to handle the communication with the TMDB API
- [x] Search TV series by name endpoint
- [x] Get TV series by id endpoint
  - [x] Caching
- [x] Error handling middleware
- [x] Structured logging
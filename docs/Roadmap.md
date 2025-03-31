# In progress

# Planned

- [ ] Add seasons collection in TvSeries struct
  - [ ] Create a DTO in contracts and use it for `/series/{id}` endpoint

- [ ] Endpoint for getting a specific season
- [ ] Endpoint for getting a specific episode

- [ ] Add authentication
  - [ ] Create sign-up endpoint
  - [ ] Create login endpoint
  - [ ] Create delete account endpoint
  - [ ] Add. JWT authentication and authorization on endpoints
  - [ ] Create TMDB sessions for users at login

- [ ] Add SQL database
  - [ ] Add migrations
- [ ] Add tracking
- [ ] Add notifications

- [ ] Get TMDB images url from the `/configuration` endpoint at startup

# Launched

- [x] TMDB client to handle the communication with the TMDB API
- [x] Search TV series by name endpoint
- [x] Get TV series by id endpoint
  - [x] Caching
- [x] Error handling middleware
- [x] Structured logging
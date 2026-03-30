# Fix plan

## Priority items (not yet implemented)

### Phase 1: Solution scaffolding
- [x] Create solution and all project skeletons with correct dependencies (Domain has zero refs to other projects) per specs/overview.md
### Phase 2: Domain layer
- [x] Implement Cat entity per specs/domain-model.md
- [x] Implement FeedingLog entity and FoodType enum per specs/domain-model.md
- [x] Implement FoodStock entity per specs/domain-model.md
- [x] Implement LitterLog entity and LitterEntryType enum per specs/domain-model.md
- [x] Implement WaterLog entity per specs/domain-model.md
- [x] Implement VetRecord entity and VetRecordType enum per specs/domain-model.md
- [x] Implement Expense entity and ExpenseCategory enum per specs/domain-model.md
- [x] Implement FoodStockService domain service per specs/domain-model.md
- [ ] Define all repository interfaces in Domain (ICatRepository, IFeedingLogRepository, IFoodStockRepository, ILitterLogRepository, IWaterLogRepository, IVetRecordRepository, IExpenseRepository) per specs/domain-model.md
- [ ] Write domain unit tests for FoodStockService per specs/testing.md

### Phase 3: Infrastructure layer
- [ ] Implement EF Core entity configurations (fluent API, one per entity) per specs/infrastructure.md
- [ ] Implement CatRepository per specs/infrastructure.md
- [ ] Implement FeedingLogRepository per specs/infrastructure.md
- [ ] Implement FoodStockRepository per specs/infrastructure.md
- [ ] Implement LitterLogRepository per specs/infrastructure.md
- [ ] Implement WaterLogRepository per specs/infrastructure.md
- [ ] Implement VetRecordRepository per specs/infrastructure.md
- [ ] Implement ExpenseRepository per specs/infrastructure.md
- [ ] Set up ASP.NET Core Identity (ApplicationUser, seeded admin account from config) per specs/infrastructure.md
- [ ] Register all repositories and DbContext in AddInfrastructure extension method per specs/infrastructure.md
- [ ] Write infrastructure integration tests for all repositories using Testcontainers per specs/testing.md

### Phase 4: Application layer
- [ ] Create DTOs for all entities (request + response DTOs)
- [ ] Implement ICatService and CatService (get, update)
- [ ] Implement IFeedingService and FeedingService (log feeding, get recent, food stock CRUD, brand autocomplete)
- [ ] Implement ILitterService and LitterService (log event, get since date, get latest, get latest full change)
- [ ] Implement IWaterService and WaterService (log clean, get recent, get latest)
- [ ] Implement IVetService and VetService (CRUD, get upcoming within date range)
- [ ] Implement IExpenseService and ExpenseService (CRUD, get by date range, monthly totals by category)
- [ ] Register all services in AddApplication extension method
- [ ] Write application unit tests for all services per specs/testing.md

### Phase 5: Blazor frontend
- [ ] Set up Web project with DI (call AddApplication and AddInfrastructure in Program.cs)
- [ ] Configure ASP.NET Core Identity cookie auth and login page per specs/blazor-frontend.md
- [ ] Apply [Authorize] to all pages; redirect unauthenticated to /login per specs/blazor-frontend.md
- [ ] Build navigation layout (top nav / sidebar, Luna's name, logout button) per specs/blazor-frontend.md
- [ ] Build Dashboard page with summary cards and quick-action buttons per specs/blazor-frontend.md
- [ ] Build Feeding Tracker page (log form, food stock list, recent feedings) per specs/blazor-frontend.md
- [ ] Build Litter Tracker page (large entry type buttons, history) per specs/blazor-frontend.md
- [ ] Build Water Tracker page (one-tap log, history) per specs/blazor-frontend.md
- [ ] Build Vet Records page (past/upcoming tabs, add/edit form) per specs/blazor-frontend.md
- [ ] Build Expenses page (summary bar, filtered list, add form) per specs/blazor-frontend.md
- [ ] Build Cat Profile page (Luna's details, edit form) per specs/blazor-frontend.md
- [ ] Responsive polish — verify all pages work on mobile (large tap targets, readable fonts) per specs/blazor-frontend.md

### Phase 6: Deployment
- [ ] Create Dockerfile for Web project
- [ ] Complete docker-compose.yml (postgres + web, health checks, env vars for connection string and seed credentials)
- [ ] Verify auto-migration on startup works end-to-end
- [ ] Verify full docker-compose up works end-to-end

## Completed
- [x] Set up test projects with NUnit + Awesome Assertions + AutoFixture + NSubstitute per specs/testing.md
- [x] Create solution and all project skeletons with correct dependencies (Domain has zero refs to other projects) per specs/overview.md
- [x] Set up docker-compose.yml with Postgres and Web containers per specs/overview.md
- [x] Configure EF Core DbContext (CatTrackerDbContext extending IdentityDbContext) and initial migration per specs/infrastructure.md

## Backlog
(items deferred to future work)
- Push notifications / reminders for feeding, vet appointments
- Multiple cat UI (currently defaults to Luna; data model supports it)
- Photo upload for Cat Profile
- Expense recurring auto-log (log next recurrence automatically)

## Known issues
(none yet)

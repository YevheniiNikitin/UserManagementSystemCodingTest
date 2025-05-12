#!/bin/bash
set -e

echo "Waiting for PostgreSQL..."
until pg_isready -h postgresql -p 5432 -U postgres; do
  sleep 1
done
echo "PostgreSQL is ready."

if ! PGPASSWORD=postgres psql -h postgresql -U postgres -tc "SELECT 1 FROM pg_database WHERE datname = 'umsdb_auth'" | grep -q 1; then
  echo "Creating database umsdb_auth..."
  PGPASSWORD=postgres psql -h postgresql -U postgres -c "CREATE DATABASE umsdb_auth;"
fi

if ! PGPASSWORD=postgres psql -h postgresql -U postgres -tc "SELECT 1 FROM pg_roles WHERE rolname = 'umsuser_auth'" | grep -q 1; then
  echo "Creating user umsuser_auth..."
  PGPASSWORD=postgres psql -h postgresql -U postgres -c "CREATE USER umsuser_auth WITH PASSWORD 'umsdbauthuserpassword';"
fi

PGPASSWORD=postgres psql -h postgresql -U postgres -c "GRANT ALL PRIVILEGES ON DATABASE umsdb_auth TO umsuser_auth;"
PGPASSWORD=postgres psql -h postgresql -U postgres -d umsdb_auth -c "GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO umsuser_auth;"
PGPASSWORD=postgres psql -h postgresql -U postgres -d umsdb_auth -c "ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO umsuser_auth;"

# Build and run EF migration bundle
cd AuthService
dotnet ef migrations bundle \
  --project ../AuthService.Database/AuthService.Database.csproj \
  --startup-project AuthService.csproj \
  --output efbundle \
  --context AuthServiceDbContext \
  --force

./efbundle --connection "Host=postgresql;Port=5432;Database=umsdb_auth;Username=postgres;Password=postgres;"

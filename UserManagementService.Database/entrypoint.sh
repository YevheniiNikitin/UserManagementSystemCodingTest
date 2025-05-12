#!/bin/bash
set -e

echo "Waiting for PostgreSQL..."
until pg_isready -h postgresql -p 5432 -U postgres; do
  sleep 1
done
echo "PostgreSQL is ready."

if ! PGPASSWORD=postgres psql -h postgresql -U postgres -tc "SELECT 1 FROM pg_database WHERE datname = 'umsdb'" | grep -q 1; then
  echo "Creating database umsdb..."
  PGPASSWORD=postgres psql -h postgresql -U postgres -c "CREATE DATABASE umsdb;"
fi

if ! PGPASSWORD=postgres psql -h postgresql -U postgres -tc "SELECT 1 FROM pg_roles WHERE rolname = 'umsuser'" | grep -q 1; then
  echo "Creating user umsuser..."
  PGPASSWORD=postgres psql -h postgresql -U postgres -c "CREATE USER umsuser WITH PASSWORD 'umsdbuserpassword';"
fi

PGPASSWORD=postgres psql -h postgresql -U postgres -c "GRANT ALL PRIVILEGES ON DATABASE umsdb TO umsuser;"
PGPASSWORD=postgres psql -h postgresql -U postgres -d umsdb -c "GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO umsuser;"
PGPASSWORD=postgres psql -h postgresql -U postgres -d umsdb -c "ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO umsuser;"

# Build and run EF migration bundle
cd UserManagementService
dotnet ef migrations bundle \
  --project ../UserManagementService.Database/UserManagementService.Database.csproj \
  --startup-project UserManagementService.csproj \
  --output efbundle \
  --context UserManagementServiceDbContext \
  --force

./efbundle --connection "Host=postgresql;Port=5432;Database=umsdb;Username=postgres;Password=postgres;"

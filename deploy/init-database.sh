#!/usr/bin/env bash
set -euo pipefail

script_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
if [[ ! -f "$script_dir/.env" ]]; then
  echo "Missing $script_dir/.env. Copy .env.example and set the passwords first." >&2
  exit 1
fi

set -a
source "$script_dir/.env"
set +a

compose=(docker compose --env-file "$script_dir/.env" -f "$script_dir/docker-compose.yml")
connection="CAMPUS/${CAMPUS_DB_PASSWORD}@FREEPDB1"

for script in \
  001_create_tables_docker.sql \
  003_views.sql \
  004_functions.sql \
  005_procedures.sql \
  006_triggers.sql
do
  echo "Running $script"
  "${compose[@]}" exec -T oracle sqlplus -L -s "$connection" "@/opt/campus-trade/ddl/$script"
done

echo "Loading seed data"
"${compose[@]}" exec -T oracle sqlplus -L -s "$connection" "@/opt/campus-trade/seed/seed_data.sql"

echo "Database initialization completed."

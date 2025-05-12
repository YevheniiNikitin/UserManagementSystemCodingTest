FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /app

RUN apt-get update && apt-get install -y postgresql-client \
    && dotnet tool install --global dotnet-ef

ENV PATH="${PATH}:/root/.dotnet/tools"

COPY . .
COPY AuthService.Database/entrypoint.sh /app/entrypoint.sh

RUN chmod +x /app/entrypoint.sh

CMD ["/app/entrypoint.sh"]
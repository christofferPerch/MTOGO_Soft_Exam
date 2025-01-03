FROM mcr.microsoft.com/mssql/server:2019-latest

USER root

# Set up directories
RUN mkdir -p /usr/src/app && chmod -R 777 /usr/src/app

# Install tools
RUN apt-get update && apt-get install -y curl apt-transport-https gnupg && \
    curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - && \
    curl https://packages.microsoft.com/config/ubuntu/20.04/prod.list > /etc/apt/sources.list.d/mssql-release.list && \
    apt-get update && ACCEPT_EULA=Y apt-get install -y mssql-tools unixodbc-dev && \
    echo 'export PATH="$PATH:/opt/mssql-tools/bin"' >> ~/.bashrc && \
    apt-get clean

USER mssql

ENV ACCEPT_EULA=Y
ENV SA_PASSWORD=YourStrong@Password1

COPY init-test-review.sql /usr/src/app/init-test-review.sql

CMD /opt/mssql/bin/sqlservr & sleep 30 && \
    until /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P YourStrong@Password1 -d master -i /usr/src/app/init-test-review.sql; do \
        echo "Waiting for SQL Server to be ready..."; \
        sleep 5; \
    done && \
    wait

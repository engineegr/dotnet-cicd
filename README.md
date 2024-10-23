# dotnet-cicd

## Usage
### Migrate DB
```
#
# Migrate DB super-app
#
- project: 'apps-team/dotnet-cicd'
  file: "/templates/db-migration.yml"
  inputs:
    stage: deploy
    shell_runner: linux
    # default_branch: dev_linux
    default_branch: develop # just for testing
    environment: development-linux
    project_root: test/LdapQueryAPI
    cs_project_path: test/LdapQueryAPI/LdapQueryAPI.csproj
    db_context: DUMMY
    app_title: super-app
    aspnetcore_environment: Production
    connection_string: "UserID=user;Password=nesecret;Host=super-db.example.com;Port=5432;Database=db_name;Pooling=true;IncludeErrorDetail=true;"
```

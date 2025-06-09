######## Build Source ########
FROM wlniao.com/wlniao/dotnet-sdk:8.0.0-alpine as publish
COPY ./ /code
WORKDIR /code
RUN dotnet publish -o /dist -c Release


######## Build Images ########
FROM wlniao.com/wlniao/dotnet:8.0.4-alpine as builder
COPY --from=publish --chmod=0777 /dist /wln
CMD ["dotnet", "app.dll"]
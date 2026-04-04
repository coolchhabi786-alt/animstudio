param location string
param environment string
param redisName string

resource redis 'Microsoft.Cache/Redis@2023-08-01' = {
  name: redisName
  location: location
  properties: {
    sku: {
      name: (environment == 'dev') ? 'Basic' : 'Standard'
      family: 'C'
      capacity: (environment == 'dev') ? 0 : 1
    }
    enableNonSslPort: false
    minimumTlsVersion: '1.2'
    redisConfiguration: {
      maxmemoryPolicy: 'allkeys-lru'
    }
  }
}

output redisId string = redis.id
output redisHostname string = redis.properties.hostName
output redisPrimaryKey string = redis.listKeys().primaryKey

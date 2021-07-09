# Cadastro de pessoas que estão infectadas usando API .NET integrada ao MongoDB 🌎

###Neste programa vamos utilizar o VS Code para construir o bando de dados, utilizando o parâmetro "GeoJson2DGeographicCoordinates" para geolocalização em latitude e longitude que será integrada ao MondoDB e o Postman para verificação do banco de dados. Servirá para cadastro de pessoas infectadas com o Coronavirus. Abaixo colocarei as principais class usadas. 



1. **_Program.cs_:** 

   using System;

   using System.Collections.Generic;

   using System.Linq;

   using System.Threading.Tasks;

   using Microsoft.AspNetCore.Hosting;

   using Microsoft.Extensions.Configuration;

   using Microsoft.Extensions.Hosting;

   using Microsoft.Extensions.Logging;

   namespace Api

   {

   ​    public class Program

   ​    {

   ​        public static void Main(string[] args)

   ​        {

   ​            CreateHostBuilder(args).Build().Run();

   ​        }

   ​        public static IHostBuilder CreateHostBuilder(string[] args) =>

   ​            Host.CreateDefaultBuilder(args)

   ​                .ConfigureWebHostDefaults(webBuilder =>

   ​                {

   ​                    webBuilder.UseStartup<Startup>();

   ​                });

   ​    }

   }

   ​

2. **_Startup.cs:_**

   using System;

   using System.Collections.Generic;

   using System.Linq;

   using System.Threading.Tasks;

   using Microsoft.AspNetCore.Builder;

   using Microsoft.AspNetCore.Hosting;

   using Microsoft.AspNetCore.HttpsPolicy;

   using Microsoft.AspNetCore.Mvc;

   using Microsoft.Extensions.Configuration;

   using Microsoft.Extensions.DependencyInjection;

   using Microsoft.Extensions.Hosting;

   using Microsoft.Extensions.Logging;

   using Microsoft.OpenApi.Models;

   namespace Api

   {

   ​    public class Startup

   ​    {

   ​        public Startup(IConfiguration configuration)

   ​        {

   ​            Configuration = configuration;

   ​        }

   ​        public IConfiguration Configuration { get; }

   ​        // This method gets called by the runtime. Use this method to add services to the container.

   ​        public void ConfigureServices(IServiceCollection services)

   ​        {

   ​            services.AddSingleton<Data.MongoDB>();

   ​            services.AddControllers();

   ​        }

   ​        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.

   ​        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)

   ​        {

   ​            app.UseHttpsRedirection();

   ​            app.UseRouting();

   ​            app.UseAuthorization();

   ​            app.UseEndpoints(endpoints =>

   ​            {

   ​                endpoints.MapControllers();

   ​            });

   ​        }

   ​    }

   }

   ​

3. **_Infectado.cs:_**

   using System;

   using MongoDB.Driver.GeoJsonObjectModel;

   namespace Api.Data.Collections

   {

   ​    public class Infectado

   ​    {

   ​        public Infectado(DateTime dataNascimento, string sexo, double latitude, double longitude)

   ​        {

   ​            this.DataNascimento = dataNascimento;

   ​            this.Sexo = sexo;

   ​            this.Localizacao = new GeoJson2DGeographicCoordinates(longitude, latitude);

   ​        }

   ​        public DateTime DataNascimento {get; set;}

   ​        public string Sexo {get; set;}

   ​        public GeoJson2DGeographicCoordinates Localizacao {get; set;}

   ​    }

   }

   ​

4. **_MongoDb.cs:_**

   using System;

   using Api.Data.Collections;

   using Microsoft.Extensions.Configuration;

   using MongoDB.Bson.Serialization;

   using MongoDB.Bson.Serialization.Conventions;

   using MongoDB.Driver;

   namespace Api.Data

   {

   ​    public class MongoDB

   ​    {

   ​    

   ​        public IMongoDatabase DB {get;}

   ​        public MongoDB(IConfiguration configuration)

   ​        {

   ​            try

   ​            {

   ​                var settings = MongoClientSettings.FromUrl(new MongoUrl(configuration["ConnectionString"]));

   ​                var client = new MongoClient(settings);

   ​                DB = client.GetDatabase(configuration["NomeBanco"]);

   ​                MapClasses();

   ​            }

   ​            catch (System.Exception)

   ​            {

   ​                Exception ex = null;

   ​                throw new MongoException("It was not possible to connect to MongoDB", ex);

   ​            }

   ​        }

   ​        private  void MapClasses()

   ​        {

   ​            var conventionPack = new ConventionPack {new CamelCaseElementNameConvention()};

   ​            ConventionRegistry.Register("camelCase", conventionPack, t => true);

   ​            if (!BsonClassMap.IsClassMapRegistered(typeof(Infectado)))

   ​            {

   ​                BsonClassMap.RegisterClassMap<Infectado>(i =>

   ​                {

   ​                    i.AutoMap();

   ​                    i.SetIgnoreExtraElements(true);

   ​                });

   ​            }

   ​        }

   ​    }

   }

   ​

5. **_InfectadoController.cs:_**

   using System;

   using Api.Data.Collections;

   using Api.Models;

   using Microsoft.AspNetCore.Mvc;

   using MongoDB.Driver;

   namespace Api.Controllers

   {

   ​    [ApiController]

   ​    [Route("Controller")]

   ​    public class InfectadoController : ControllerBase

   ​    {

   ​        Data.MongoDB _mongoDB;

   ​        IMongoCollection<Infectado> _infectadoCollection;

   ​        public InfectadoController(Data.MongoDB mongoDB)

   ​        {

   ​            _mongoDB = mongoDB;

   ​            _infectadoCollection = _mongoDB.DB.GetCollection<Infectado>(typeof(Infectado).Name.ToLower());

   ​        }

   ​        [HttpPost]

   ​        public ActionResult SalvarInfectado([FromBody] InfectadoDto dto)

   ​        {

   ​            var infectados = new Infectado(dto.DataNascimento, dto.Sexo, dto.Latitude, dto.Longitude);

   ​            _infectadoCollection.InsertOne(infectados);

   ​            return StatusCode(201, "Infectado adicionado com sucesso");

   ​        }

   ​        [HttpGet]

   ​        public ActionResult ObterInfectado()

   ​        {

   ​            var infectados = _infectadoCollection.Find(Builders<Infectado>.Filter.Empty).ToList();

   ​            return Ok(infectados);

   ​        }

   ​        [HttpPut]

   ​        public ActionResult AtualizarInfectado([FromBody] InfectadoDto dto)

   ​        {

   ​            var infectado = new Infectado(dto.DataNascimento, dto.Sexo, dto.Latitude, dto.Longitude);

   ​            _infectadoCollection.UpdateOne(Builders<Infectado>.Filter.Where(_ => _.DataNascimento == dto.DataNascimento), Builders<Infectado>.Update.Set("sexo", dto.Sexo));

   ​            return Ok("Atualizado com sucesso");

   ​        }

   ​        [HttpDelete("{dataNasc}")]

   ​        public ActionResult Delete(DateTime dataNasc)

   ​        {

   ​            

   ​            _infectadoCollection.DeleteOne(Builders<Infectado>.Filter.Where(_ => _.DataNascimento == dataNasc));

   ​            return Ok("Atualizado com sucesso");

   ​        }

   ​    }

   }

   ​

6. **_InfectadoDto.cs:_**

   using System;

   namespace Api.Models

   {

   ​    public class InfectadoDto

   ​    {

   ​        public DateTime DataNascimento {get; set;}

   ​        public string Sexo {get; set;}

   ​        public double Latitude {get; set;}

   ​        public double Longitude {get; set;}

   ​    }

   }
using System;
using Api.Data.Collections;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Api.Controllers
{
    [ApiController]
    [Route("Controller")]
    public class InfectadoController : ControllerBase
    {
        Data.MongoDB _mongoDB;

        IMongoCollection<Infectado> _infectadoCollection;

        public InfectadoController(Data.MongoDB mongoDB)
        {
            _mongoDB = mongoDB;
            _infectadoCollection = _mongoDB.DB.GetCollection<Infectado>(typeof(Infectado).Name.ToLower());
        }

        [HttpPost]

        public ActionResult SalvarInfectado([FromBody] InfectadoDto dto)
        {
            var infectados = new Infectado(dto.DataNascimento, dto.Sexo, dto.Latitude, dto.Longitude);

            _infectadoCollection.InsertOne(infectados);

            return StatusCode(201, "Infectado adicionado com sucesso");
        }

        [HttpGet]
        public ActionResult ObterInfectado()
        {
            var infectados = _infectadoCollection.Find(Builders<Infectado>.Filter.Empty).ToList();

            return Ok(infectados);
        }

        [HttpPut]
        public ActionResult AtualizarInfectado([FromBody] InfectadoDto dto)
        {
            var infectado = new Infectado(dto.DataNascimento, dto.Sexo, dto.Latitude, dto.Longitude);
            _infectadoCollection.UpdateOne(Builders<Infectado>.Filter.Where(_ => _.DataNascimento == dto.DataNascimento), Builders<Infectado>.Update.Set("sexo", dto.Sexo));

            return Ok("Atualizado com sucesso");
        }

        [HttpDelete("{dataNasc}")]
        public ActionResult Delete(DateTime dataNasc)
        {
            
            _infectadoCollection.DeleteOne(Builders<Infectado>.Filter.Where(_ => _.DataNascimento == dataNasc));

            return Ok("Atualizado com sucesso");
        }
    }
}
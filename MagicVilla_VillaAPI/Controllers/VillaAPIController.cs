﻿using AutoMapper;
using MagicVilla_VillaAPI.Data;
//using MagicVilla_VillaAPI.Logging;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace MagicVilla_VillaAPI.Controllers
{
    //[Route("api/[controller]")]
    [Route("api/VillaAPI")]
    [ApiController]
    public class VillaAPIController: ControllerBase
    {
        //private readonly ILogging _logger;
        //public VillaAPIController(ILogging logger)

        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public VillaAPIController(ApplicationDbContext db, IMapper mapper)
        {
            //_logger = logger;
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VillaDTO>>> GetVillas() 
        {
            //_logger.Log("Getting all villas","");
            //return Ok(VillaStore.villaList);
            IEnumerable<Villa> villaList = await _db.Villas.ToListAsync();
            return Ok(_mapper.Map<List<VillaDTO>>(villaList));
        }
        [HttpGet("{id:int}",Name ="GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(200, Type = typeof(VillaDTO)]
        //[ProducesResponseType(200]
        //[ProducesResponseType(404)]
        //[ProducesResponseType(400)]
        public async Task<ActionResult< VillaDTO >> GetVilla(int id)
        {
            if(id == 0)
            {
                //_logger.Log("Get Villa Error with Id " + id, "error");
                return BadRequest();
            }
            //var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            var villa =await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);
            if(villa == null) { 
                return NotFound();
            }
            return Ok(_mapper.Map<VillaDTO>(villa));

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task< ActionResult<VillaDTO>> CreateVilla([FromBody]VillaCreateDTO createDTO) {
            //not required since APIController handles this in models VillaDTO.cs file
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}
            //if(VillaStore.villaList.FirstOrDefault(u=>u.Name.ToLower() == villaDTO.Name.ToLower()) != null)
            if(await _db.Villas.FirstOrDefaultAsync(u=>u.Name.ToLower() == createDTO.Name.ToLower()) != null)
            {
                ModelState.AddModelError("CustomError", "Villa Already Exists!");
                return BadRequest(ModelState);
            }
            if(createDTO == null)
            {
                return BadRequest(createDTO);
            }
            //if(villaDTO.Id > 0)
            //{
            //    return StatusCode(StatusCodes.Status500InternalServerError);
            //}
            //villaDTO.Id = VillaStore.villaList.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;
            Villa model = _mapper.Map<Villa>(createDTO);
            //Villa model = new()
            //{
            //    Amenity = createDTO.Amenity,
            //    Details = createDTO.Details,
            //    ImageUrl = createDTO.ImageUrl,
            //    Name = createDTO.Name,
            //    Occupancy = createDTO.Occupancy,
            //    Rate = createDTO.Rate,
            //    Sqft = createDTO.Sqft
            //};
            await _db.Villas.AddAsync(model);
            await _db.SaveChangesAsync();
            //VillaStore.villaList.Add(villaDTO);
            return CreatedAtRoute("GetVilla", new {id= model.Id} ,model);    
        }

        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult DeleteVilla(int id)
        {
            if (id==0)
            {
                return BadRequest();
            }
            //var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            var villa = _db.Villas.FirstOrDefault(u => u.Id == id);
            if (villa==null)
            {
                return NotFound();
            }
            //VillaStore.villaList.Remove(villa);
            _db.Villas.Remove(villa);
            _db.SaveChanges();
            return NoContent();
        }

        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task< IActionResult> UpdateVila(int id, [FromBody]VillaUpdateDTO updateDTO)
        {
            if(updateDTO==null || id != updateDTO.Id)
            {
                return BadRequest();
            }
            //var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            //villa.Name= villaDTO.Name;
            //villa.Sqft= villaDTO.Sqft;
            //villa.Occupancy= villaDTO.Occupancy;

            Villa model = _mapper.Map<Villa>(updateDTO);

            //Villa model = new()
            //{
            //    Amenity = updateDTO.Amenity,
            //    Details = updateDTO.Details,
            //    ImageUrl = updateDTO.ImageUrl,
            //    Id = updateDTO.Id,
            //    Name = updateDTO.Name,
            //    Occupancy = updateDTO.Occupancy,
            //    Rate = updateDTO.Rate,
            //    Sqft = updateDTO.Sqft
            //}; 
            _db.Villas.Update(model);
            await _db.SaveChangesAsync();

            return NoContent() ;
        }

        [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdatePartialVila(int id, JsonPatchDocument<VillaUpdateDTO> patchDTO)
        {
            if (patchDTO == null || id ==0)
            {
                return BadRequest();
            }
            //var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            var villa = await _db.Villas.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

            VillaUpdateDTO villaDTO =_mapper.Map<VillaUpdateDTO>(villa);    
            //VillaUpdateDTO villaDTO = new()
            //{
            //    Amenity = villa.Amenity,
            //    Details = villa.Details,
            //    ImageUrl = villa.ImageUrl,
            //    Name = villa.Name,
            //    Occupancy = villa.Occupancy,
            //    Rate = villa.Rate,
            //    Sqft = villa.Sqft
            //};
            if (villa == null)
            {
                return BadRequest(); //return NotFound();
            }
            //patchDTO.ApplyTo(villa, ModelState);
            patchDTO.ApplyTo(villaDTO, ModelState);
            Villa model = _mapper.Map<Villa>(villaDTO);
            //Villa model = new()
            //{
            //    Amenity = villaDTO.Amenity,
            //    Details = villaDTO.Details,
            //    ImageUrl = villaDTO.ImageUrl,
            //    Name = villaDTO.Name,
            //    Occupancy = villaDTO.Occupancy,
            //    Rate = villaDTO.Rate,
            //    Sqft = villaDTO.Sqft
            //};
            _db.Villas.Update(model);
            await _db.SaveChangesAsync();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return NoContent();
        }

    }
}

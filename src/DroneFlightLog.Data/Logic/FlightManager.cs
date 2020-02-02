﻿using System;
using System.Collections.Generic;
using System.Linq;
using DroneFlightLog.Data.Entities;
using DroneFlightLog.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DroneFlightLog.Data.Logic
{
    internal class FlightManager<T> : IFlightManager where T : DbContext, IDroneFlightLogDbContext
    {
        private readonly IDroneFlightLogFactory<T> _factory;

        internal FlightManager(IDroneFlightLogFactory<T> factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Add a flight, given its details
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="droneId"></param>
        /// <param name="locationId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public Flight AddFlight(int operatorId, int droneId, int locationId, DateTime start, DateTime end)
        {
            // These calls will throw an exception if the entity with the specified ID doesn't exist
            _factory.Operators.GetOperator(operatorId);
            _factory.Drones.GetDrone(droneId);
            _factory.Locations.GetLocation(locationId);

            Flight flight = new Flight
            {
                OperatorId = operatorId,
                DroneId = droneId,
                LocationId = locationId,
                Start = start,
                End = end
            };

            _factory.Context.Flights.Add(flight);
            return flight;
        }

        /// <summary>
        /// Find flights matching the specified filtering criteria and return the specified
        /// page of results
        /// </summary>
        /// <param name="operatorId"></param>
        /// <param name="droneId"></param>
        /// <param name="locationId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IEnumerable<Flight> FindFlights(int? operatorId, int? droneId, int? locationId, DateTime? start, DateTime? end, int pageNumber, int pageSize)
        {
            IEnumerable<Flight> flights = _factory.Context.Flights
                                                          .Include(f => f.Drone)
                                                            .ThenInclude(d => d.Model)
                                                                .ThenInclude(m => m.Manufacturer)
                                                          .Include(f => f.Location)
                                                          .Include(f => f.Operator)
                                                            .ThenInclude(o => o.Address)
                                                          .Where(f =>   ((operatorId == null) || (operatorId == f.OperatorId)) &&
                                                                        ((droneId == null) || (droneId == f.DroneId)) &&
                                                                        ((locationId == null) || (locationId == f.LocationId)) &&
                                                                        ((start == null) || (f.Start >= start)) &&
                                                                        ((end == null) || (f.End <= end)))
                                                          .Skip((pageNumber - 1) * pageSize)
                                                          .Take(pageSize);

            return flights;
        }
    }
}
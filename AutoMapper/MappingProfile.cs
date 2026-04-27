using AutoMapper;
using WorkOrderApp.Controllers;
using WorkOrderApp.Entities;

namespace WorkOrderApp.AutoMapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ── Base ──────────────────────────────────────────────────────────
            CreateMap<BaseEntity, BaseDetails>();

            // ── User ──────────────────────────────────────────────────────────
            CreateMap<CreateUserDto, User>()
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore());

            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.Id,              opt => opt.Ignore())
                .ForMember(dest => dest.Active,          opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt,       opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt,       opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash,    opt => opt.Ignore())
                .ForMember(dest => dest.UserLocations,   opt => opt.Ignore());

            CreateMap<User, UserDetails>();

            // ── Location ──────────────────────────────────────────────────────
            CreateMap<CreateLocationDto, Location>();

            CreateMap<UpdateLocationDto, Location>()
                .ForMember(dest => dest.Id,            opt => opt.Ignore())
                .ForMember(dest => dest.Active,        opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt,     opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt,     opt => opt.Ignore())
                .ForMember(dest => dest.UserLocations, opt => opt.Ignore());

            CreateMap<Location, LocationDetails>();

            // ── Request ───────────────────────────────────────────────────────
            CreateMap<CreateRequestDto, Request>();

            CreateMap<Request, RequestDetails>()
                .ForMember(dest => dest.Status,   opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()));

            CreateMap<RequestLog, RequestLogDetails>();

            // ── WorkOrder ─────────────────────────────────────────────────────
            CreateMap<CreateWorkOrderDto, WorkOrder>();

            CreateMap<WorkOrder, WorkOrderDetails>()
                .ForMember(dest => dest.Status,   opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()));

            CreateMap<WorkOrderLog, WorkOrderLogDetails>();

            // ── Cost ──────────────────────────────────────────────────────────
            CreateMap<CreateCostDto, Cost>();
            CreateMap<Cost, CostDetails>();

            // ── Part ──────────────────────────────────────────────────────────
            CreateMap<CreatePartDto, Part>();
            CreateMap<Part, PartDetails>();

            // ── Attachment ────────────────────────────────────────────────────
            CreateMap<CreateAttachmentDto, Attachment>();
            CreateMap<Attachment, AttachmentDetails>();

            // ── Generic enum base ─────────────────────────────────────────────
            CreateMap<CreateEnumDto, BaseEnum>();
            CreateMap<UpdateEnumDto, BaseEnum>();
            CreateMap<BaseEnum, EnumDetails>();

            // ── Enum subtypes ─────────────────────────────────────────────────
            CreateMap<CreateEnumDto, Appliance>();
            CreateMap<UpdateEnumDto, Appliance>();
            CreateMap<Appliance, EnumDetails>();

            CreateMap<CreateEnumDto, IdentificationType>();
            CreateMap<UpdateEnumDto, IdentificationType>();
            CreateMap<IdentificationType, EnumDetails>();

            CreateMap<CreateEnumDto, PaymentType>();
            CreateMap<UpdateEnumDto, PaymentType>();
            CreateMap<PaymentType, EnumDetails>();

            CreateMap<CreateEnumDto, LeaseType>();
            CreateMap<UpdateEnumDto, LeaseType>();
            CreateMap<LeaseType, EnumDetails>();

            CreateMap<CreateEnumDto, FeeType>();
            CreateMap<UpdateEnumDto, FeeType>();
            CreateMap<FeeType, EnumDetails>();

            CreateMap<CreateEnumDto, Role>();
            CreateMap<UpdateEnumDto, Role>();
            CreateMap<Role, EnumDetails>();

            CreateMap<CreatePropertyTypeDto, PropertyType>();
            CreateMap<UpdatePropertyTypeDto, PropertyType>()
                .ForMember(dest => dest.Id,        opt => opt.Ignore())
                .ForMember(dest => dest.Active,    opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<PropertyType, PropertyTypeDetails>();

            CreateMap<CreateEnumDto, Currency>();
            CreateMap<UpdateEnumDto, Currency>()
                .ForMember(dest => dest.Id,        opt => opt.Ignore())
                .ForMember(dest => dest.Active,    opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
            CreateMap<Currency, EnumDetails>();
        }
    }
}

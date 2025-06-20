using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AspNetPostgresAuth.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdateTriggerForPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 트리거 함수 생성
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION update_updated_at_column()
                RETURNS TRIGGER AS $$
                BEGIN
                    NEW.""UpdatedAt"" = CURRENT_TIMESTAMP;
                    RETURN NEW;
                END;
                $$ LANGUAGE plpgsql;
            ");

            // Posts 테이블에 트리거 적용
            migrationBuilder.Sql(@"
                CREATE TRIGGER update_posts_updated_at
                    BEFORE UPDATE ON ""Posts""
                    FOR EACH ROW
                    EXECUTE FUNCTION update_updated_at_column();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // 트리거 제거
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS update_posts_updated_at ON ""Posts"";");
            
            // 함수 제거
            migrationBuilder.Sql(@"DROP FUNCTION IF EXISTS update_updated_at_column();");
        }
    }
}
